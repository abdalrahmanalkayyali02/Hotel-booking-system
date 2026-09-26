namespace HBS.API.Db.Seeds;

public static class SeedValidator
{
    internal static IReadOnlyList<SeedIssue> ValidateRegionSource(GeographySource source)
    {
        var issues = new List<SeedIssue>();
        var regionIds = new HashSet<string>();
        foreach (var (kind, rows) in new[] { ("region", source.Regions), ("subregion", source.SubRegions) })
        {
            if (rows is null)
            {
                issues.Add(new("MalformedData", kind, "", "Source collection must not be null."));
                continue;
            }
            var found = new HashSet<string>();
            foreach (var row in rows)
            {
                if (row is null || string.IsNullOrWhiteSpace(row.SourceId))
                {
                    issues.Add(new("MalformedData", kind, "", "Source row and ID are required."));
                    continue;
                }
                if (!found.Add(row.SourceId))
                    issues.Add(new("DuplicateSource", kind, row.SourceId, "Duplicate CSV source ID."));
                if (string.IsNullOrWhiteSpace(row.Name))
                    issues.Add(new("RequiredName", kind, row.SourceId, "Source name is required."));
                else if (row.Name.Length > 50)
                    issues.Add(new("MaxLength", kind, row.SourceId, "Source name exceeds 50 characters."));
                if (kind == "region") regionIds.Add(row.SourceId);
                else if (row.RegionSourceId is null || !regionIds.Contains(row.RegionSourceId))
                    issues.Add(new("ForeignKey", kind, row.SourceId, "Subregion references an unknown region."));
            }
        }
        return issues;
    }

    public static bool IsUuid7(Guid id)
    {
        var text = id.ToString("D");
        return id != Guid.Empty && text[14] == '7' && "89ab".Contains(text[19]);
    }

    public static IReadOnlyList<SeedIssue> Validate(SeedData data, IReadOnlyDictionary<string, Guid> ids,
        GeographySource source, IReadOnlyDictionary<string, TranslationCorrection> corrections)
    {
        var issues = new List<SeedIssue>();
        void Error(string code, string kind, string key, string message) => issues.Add(new(code, kind, key, message));
        foreach (var property in typeof(SeedData).GetProperties())
        {
            if (property.GetValue(data) is not System.Collections.IEnumerable collection)
                Error("MalformedData", property.Name, "", "Collection must not be null.");
            else
                foreach (var item in collection)
                    if (item is null) Error("MalformedData", property.Name, "", "Collection contains a null row.");
        }
        if (issues.Count != 0) return issues;
        issues.AddRange(ValidateRegionSource(source));
        if (issues.Count != 0) return issues;
        var mappingIds = new HashSet<Guid>();
        foreach (var (key, id) in ids)
        {
            if (!IsUuid7(id)) Error("UuidVersion", "mapping", key, "Expected RFC UUIDv7.");
            if (!mappingIds.Add(id)) Error("DuplicateId", "mapping", key, "ID is assigned to multiple source keys.");
        }
        var requiredKeys = new List<string> { "language/en", "language/ar" };
        foreach (var (kind, rows) in new[] { ("region", source.Regions), ("subregion", source.SubRegions) })
            foreach (var row in rows)
            {
                requiredKeys.Add(SeedKeys.Entity(kind, row.SourceId));
                requiredKeys.Add(SeedKeys.Translation(kind, row.SourceId, "en"));
                requiredKeys.Add(SeedKeys.Translation(kind, row.SourceId, "ar"));
            }
        foreach (var (kind, rows) in new[] { ("country", source.Countries), ("state", source.States), ("city", source.Cities),
            ("role", SeedGenerator.Roles.Select(r => new GeographyRow { SourceId = r.Code, Name = r.English }).ToList()) })
        {
            foreach (var row in rows)
            {
                requiredKeys.Add(SeedKeys.Entity(kind, row.SourceId));
                requiredKeys.Add(SeedKeys.Translation(kind, row.SourceId, "en"));
                requiredKeys.Add(SeedKeys.Translation(kind, row.SourceId, "ar"));
            }
        }
        foreach (var key in requiredKeys)
            if (!ids.ContainsKey(key)) Error("MissingMapping", "mapping", key, "Persistent identity mapping is missing.");
        var correctionKeys = source.Countries.Select(r => SeedKeys.Entity("country", r.SourceId))
            .Concat(source.States.Select(r => SeedKeys.Entity("state", r.SourceId)))
            .Concat(source.Cities.Select(r => SeedKeys.Entity("city", r.SourceId)))
            .Concat(source.Regions.Select(r => SeedKeys.Entity("region", r.SourceId)))
            .Concat(source.SubRegions.Select(r => SeedKeys.Entity("subregion", r.SourceId))).ToHashSet();
        foreach (var key in corrections.Keys)
            if (!correctionKeys.Contains(key)) Error("UnexpectedCorrection", "correction", key, "Correction has no source entity.");
        if (issues.Count != 0) return issues;
        Guid Lookup(string key)
        {
            if (ids.TryGetValue(key, out var id)) return id;
            Error("MissingMapping", "mapping", key, "Persistent identity mapping is missing.");
            return Guid.Empty;
        }
        var allIds = new HashSet<Guid>();
        void CheckId(Guid id, string kind)
        {
            if (!IsUuid7(id)) Error("UuidVersion", kind, id.ToString(), "Expected RFC UUIDv7.");
            if (!allIds.Add(id)) Error("DuplicateId", kind, id.ToString(), "Duplicate entity or translation ID.");
        }
        void Entities<T>(string kind, IEnumerable<T> actual, IEnumerable<GeographyRow> expected,
            Func<T, Guid> getId, Func<T, string?> getName, int? maxLength = null)
        {
            var names = new Dictionary<Guid, GeographyRow>();
            foreach (var row in expected)
            {
                var id = Lookup(SeedKeys.Entity(kind, row.SourceId));
                if (!names.TryAdd(id, row)) Error("DuplicateSource", kind, row.SourceId, "Duplicate source or mapping.");
            }
            var found = new HashSet<Guid>();
            foreach (var item in actual)
            {
                var id = getId(item);
                CheckId(id, kind);
                if (maxLength is int limit && getName(item)?.Length > limit)
                    Error("MaxLength", kind, id.ToString(), $"Name exceeds {limit} characters.");
                found.Add(id);
                if (!names.TryGetValue(id, out var row)) Error("UnexpectedEntity", kind, id.ToString(), "Entity has no source mapping.");
                else if (getName(item) != row.Name || string.IsNullOrWhiteSpace(getName(item)))
                    Error("NameMismatch", kind, row.SourceId, "Required name differs from source.");
            }
            foreach (var (id, row) in names)
                if (!found.Contains(id)) Error("MissingEntity", kind, row.SourceId, $"Missing {row.Name}.");
        }
        Entities("language", data.Languages, [new() { SourceId = "en", Name = "English" }, new() { SourceId = "ar", Name = "العربية" }], x => x.Id, x => x.LanguageName, 50);
        Entities("region", data.Regions, source.Regions, x => x.Id, x => x.RegionName, 50);
        Entities("subregion", data.SubRegions, source.SubRegions, x => x.Id, x => x.Name, 50);
        Entities("country", data.Countries, source.Countries, x => x.Id, x => x.CountryName, 50);
        Entities("state", data.States, source.States, x => x.Id, x => x.StateName, 50);
        Entities("city", data.Cities, source.Cities, x => x.Id, x => x.CityName, 100);
        var roleRows = SeedGenerator.Roles.Select(r => new GeographyRow { SourceId = r.Code, Name = r.English, ArabicName = r.Arabic }).ToList();
        var roleNames = SeedGenerator.Roles.ToDictionary(r => Lookup(SeedKeys.Entity("role", r.Code)), r => r.English);
        Entities("role", data.Roles, roleRows, x => x.Id, x => roleNames.GetValueOrDefault(x.Id));

        var countries = data.Countries.Select(x => x.Id).ToHashSet();
        var states = data.States.Select(x => x.Id).ToHashSet();
        var regions = data.Regions.Select(x => x.Id).ToHashSet();
        var sourceSubRegions = source.SubRegions.ToDictionary(r => Lookup(SeedKeys.Entity("subregion", r.SourceId)));
        foreach (var subRegion in data.SubRegions)
        {
            if (!regions.Contains(subRegion.RegionId))
                Error("ForeignKey", "subregion", subRegion.Id.ToString(), "Region does not exist.");
            if (sourceSubRegions.TryGetValue(subRegion.Id, out var row)
                && subRegion.RegionId != Lookup(SeedKeys.Entity("region", row.RegionSourceId!)))
                Error("Relationship", "subregion", row.SourceId, $"{row.Name}: region differs from CSV source.");
        }
        var sourceCities = source.Cities.ToDictionary(r => Lookup(SeedKeys.Entity("city", r.SourceId)));
        var sourceStates = source.States.ToDictionary(r => r.SourceId);
        var sourceCountries = source.Countries.ToDictionary(r => r.SourceId);
        var sourceStatesById = source.States.ToDictionary(r => Lookup(SeedKeys.Entity("state", r.SourceId)));
        foreach (var state in data.States)
        {
            if (!countries.Contains(state.CountryId))
                Error("ForeignKey", "state", state.Id.ToString(), "Country does not exist.");
            if (sourceStatesById.TryGetValue(state.Id, out var row)
                && (row.CountrySourceId is null || !sourceCountries.ContainsKey(row.CountrySourceId)
                    || state.CountryId != Lookup(SeedKeys.Entity("country", row.CountrySourceId))))
                Error("Relationship", "state", row.SourceId, $"{row.Name}: country differs from source hierarchy.");
        }
        foreach (var city in data.Cities)
        {
            if (!countries.Contains(city.CountryId) || !states.Contains(city.StateId) || !regions.Contains(city.RegionId))
                Error("ForeignKey", "city", city.Id.ToString(), "Country, state or region does not exist.");
            if (sourceCities.TryGetValue(city.Id, out var row))
            {
                if (row.CountrySourceId is null || row.StateSourceId is null || row.RegionSourceId is null
                    || city.CountryId != Lookup(SeedKeys.Entity("country", row.CountrySourceId))
                    || city.StateId != Lookup(SeedKeys.Entity("state", row.StateSourceId))
                    || city.RegionId != Lookup(SeedKeys.Entity("region", row.RegionSourceId))
                    || !sourceStates.TryGetValue(row.StateSourceId, out var state) || state.CountrySourceId != row.CountrySourceId
                    || !sourceCountries.TryGetValue(row.CountrySourceId, out var country) || country.RegionSourceId != row.RegionSourceId)
                    Error("Relationship", "city", row.SourceId, $"{row.Name}: country/state/region differs from source hierarchy.");
            }
        }

        var en = Lookup("language/en");
        var ar = Lookup("language/ar");
        var languages = data.Languages.Select(x => x.Id).ToHashSet();
        void Translations(string kind, IEnumerable<(Guid Id, Guid ParentId, Guid LanguageId, string Name)> actual,
            List<GeographyRow> rows, HashSet<Guid> parents, int maxLength = 50)
        {
            var expected = new Dictionary<(Guid, Guid), (Guid Id, string? Name, GeographyRow Row)>();
            foreach (var row in rows)
            {
                var parent = Lookup(SeedKeys.Entity(kind, row.SourceId));
                expected[(parent, en)] = (Lookup(SeedKeys.Translation(kind, row.SourceId, "en")), row.Name, row);
                var name = kind == "role" ? row.ArabicName : SeedGenerator.Arabic(kind, row, corrections);
                expected[(parent, ar)] = (Lookup(SeedKeys.Translation(kind, row.SourceId, "ar")), name, row);
                if (name is null) Error("MissingArabic", kind, row.SourceId, $"{row.Name}: sourced or explicitly generated Arabic name required.");
            }
            var found = new HashSet<(Guid, Guid)>();
            foreach (var t in actual)
            {
                CheckId(t.Id, kind + "-translation");
                var pair = (t.ParentId, t.LanguageId);
                if (!found.Add(pair)) Error("DuplicateTranslation", kind, t.ParentId.ToString(), "Duplicate entity/language pair.");
                if (!parents.Contains(t.ParentId) || !languages.Contains(t.LanguageId))
                    Error("ForeignKey", kind + "-translation", t.Id.ToString(), "Parent or language does not exist.");
                if (string.IsNullOrWhiteSpace(t.Name)) Error("RequiredName", kind, t.Id.ToString(), "Translation name is empty.");
                if (t.Name?.Length > maxLength)
                    Error("MaxLength", kind, t.Id.ToString(), $"Translation name exceeds {maxLength} characters.");
                if (!expected.TryGetValue(pair, out var e)) Error("UnexpectedTranslation", kind, t.Id.ToString(), "Unexpected parent/language pair.");
                else
                {
                    if (t.Id != e.Id) Error("MappingMismatch", kind, e.Row.SourceId, "Translation ID differs from persisted mapping.");
                    if (e.Name is null || t.Name != e.Name) Error("UnverifiedTranslation", kind, e.Row.SourceId, "Translation differs from sourced or reviewed name.");
                    if (t.LanguageId == ar && !SeedGenerator.HasArabic(t.Name)) Error("ArabicScript", kind, e.Row.SourceId, "Arabic translation contains no Arabic letters.");
                }
            }
            foreach (var (pair, e) in expected)
                if (e.Name is not null && !found.Contains(pair))
                    Error("MissingTranslation", kind, e.Row.SourceId, $"{e.Row.Name}: missing {(pair.Item2 == en ? "English" : "Arabic")} translation.");
        }
        Translations("region", data.RegionsTranslations.Select(t => (t.Id, t.RegionId, t.LanguageId, t.Name)),
            source.Regions, regions);
        Translations("subregion", data.SubRegionsTranslations.Select(t => (t.Id, t.SubRegionId, t.LanguageId, t.Name)),
            source.SubRegions, data.SubRegions.Select(x => x.Id).ToHashSet());
        Translations("country", data.CountriesTranslations.Select(t => (t.Id, t.CountryId, t.LanguageId, t.Name)), source.Countries, countries);
        Translations("state", data.StatesTranslations.Select(t => (t.Id, t.StateId, t.LanguageId, t.Name)), source.States, states);
        Translations("city", data.CitiesTranslations.Select(t => (t.Id, t.CityId, t.LanguageId, t.Name)), source.Cities, data.Cities.Select(x => x.Id).ToHashSet(), 100);
        Translations("role", data.RoleTranslations.Select(t => (t.Id, t.RoleId, t.LanguageId, t.Name)), roleRows, data.Roles.Select(x => x.Id).ToHashSet(), 15);
        foreach (var t in data.RoleTranslations)
        {
            if (string.IsNullOrWhiteSpace(t.Description)) Error("RequiredDescription", "role", t.RoleId.ToString(), "Role translation description is empty.");
            if (t.Description?.Length > 255) Error("MaxLength", "role", t.RoleId.ToString(), "Role description exceeds 255 characters.");
        }
        return issues;
    }
}
