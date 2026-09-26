using System.Text.RegularExpressions;
using HBS.API.Db.models;

namespace HBS.API.Db.Seeds;

public static class SeedGenerator
{
    internal static readonly (string Code, string English, string Arabic, string EnglishDescription, string ArabicDescription)[] Roles =
    [
        ("super_admin", "Super Admin", "مدير النظام", "System administration role.", "دور إدارة النظام."),
        ("hotel_admin", "Hotel Admin", "مدير الفندق", "Hotel administration role.", "دور إدارة الفندق."),
        ("receptionist", "Receptionist", "موظف الاستقبال", "Hotel reception role.", "دور موظف استقبال الفندق."),
        ("customer", "Customer", "عميل", "Registered customer role.", "دور العميل المسجل."),
        ("guest", "Guest", "مستخدم زائر", "Guest user role.", "دور المستخدم الزائر.")
    ];

    // An Arabic-script check catches empty/Latin fallbacks; it does not certify linguistic accuracy.
    internal static bool HasArabic(string? name) => !string.IsNullOrWhiteSpace(name)
        && Regex.IsMatch(name, "[\u0621-\u064A\u066E-\u06D3]");

    internal static string? Arabic(string kind, GeographyRow row, IReadOnlyDictionary<string, TranslationCorrection> corrections)
    {
        if (corrections.TryGetValue(SeedKeys.Entity(kind, row.SourceId), out var correction))
        {
            if (!HasArabic(correction.Name) || string.IsNullOrWhiteSpace(correction.ReviewedBy)
                || correction.SourceType is not ("reference" or "wikidata" or "generated")
                || (correction.SourceType == "generated" && string.IsNullOrWhiteSpace(correction.GenerationMethod))
                || !Uri.TryCreate(correction.SourceUrl, UriKind.Absolute, out var url)
                || url.Scheme is not ("http" or "https"))
                throw new InvalidDataException($"Invalid reviewed correction: {kind}/{row.SourceId}");
            if (HasArabic(row.ArabicName) && correction.Name.Trim() != row.ArabicName!.Trim())
                throw new InvalidDataException($"Correction would replace an accepted source Arabic name: {kind}/{row.SourceId}");
            return correction.Name.Trim();
        }
        return HasArabic(row.ArabicName) ? row.ArabicName!.Trim() : null;
    }

    public static async Task<IReadOnlyList<SeedIssue>> GenerateAsync(string root, CancellationToken token = default)
    {
        var source = await SeedFiles.ReadSourceAsync(root, token);
        var sourceIssues = SeedValidator.ValidateRegionSource(source);
        if (sourceIssues.Count != 0)
            throw new InvalidDataException($"Invalid region/subregion source: {sourceIssues[0].Message}");
        var mappingPath = Path.Combine(root, "id-map.json.gz");
        if (!File.Exists(mappingPath) && File.Exists(Path.Combine(root, "Staging", "seed-data.json.gz")))
            throw new InvalidDataException("ID mapping is missing. Restore it instead of regenerating existing IDs.");
        var ids = File.Exists(mappingPath)
            ? await SeedFiles.ReadAsync<SortedDictionary<string, Guid>>(mappingPath, token) : new(StringComparer.Ordinal);
        if (ids.Values.Any(id => !SeedValidator.IsUuid7(id)) || ids.Values.Distinct().Count() != ids.Count)
            throw new InvalidDataException("Existing ID mapping contains duplicate or non-UUIDv7 IDs.");
        var corrections = await SeedFiles.ReadAsync<Dictionary<string, TranslationCorrection>>(Path.Combine(root, "Sources", "arabic-corrections.json"), token);
        var knownKeys = source.Countries.Select(r => SeedKeys.Entity("country", r.SourceId))
            .Concat(source.States.Select(r => SeedKeys.Entity("state", r.SourceId)))
            .Concat(source.Cities.Select(r => SeedKeys.Entity("city", r.SourceId)))
            .Concat(source.Regions.Select(r => SeedKeys.Entity("region", r.SourceId)))
            .Concat(source.SubRegions.Select(r => SeedKeys.Entity("subregion", r.SourceId))).ToHashSet();
        if (corrections.Keys.Any(key => !knownKeys.Contains(key)))
            throw new InvalidDataException("An Arabic correction references an unknown source entity.");
        Guid Id(string key)
        {
            if (!ids.TryGetValue(key, out var id)) ids[key] = id = Guid.CreateVersion7();
            return id;
        }
        Guid Entity(string kind, string key) => Id(SeedKeys.Entity(kind, key));
        var data = new SeedData();
        var en = Entity("language", "en");
        var ar = Entity("language", "ar");
        data.Languages.AddRange([new() { Id = en, LanguageName = "English", Code = "en" }, new() { Id = ar, LanguageName = "العربية", Code = "ar" }]);
        foreach (var row in source.Regions)
            data.Regions.Add(new() { Id = Entity("region", row.SourceId), RegionName = row.Name });
        foreach (var row in source.SubRegions)
            data.SubRegions.Add(new()
            {
                Id = Entity("subregion", row.SourceId),
                Name = row.Name,
                RegionId = Entity("region", row.RegionSourceId!)
            });
        // English names remain CSV-derived; Arabic names come from deterministic local corrections.
        foreach (var row in source.Regions)
            foreach (var t in Translations("region", row))
                data.RegionsTranslations.Add(new()
                {
                    Id = t.Id,
                    RegionId = Entity("region", row.SourceId),
                    LanguageId = t.LanguageId,
                    Name = t.Name
                });
        foreach (var row in source.SubRegions)
            foreach (var t in Translations("subregion", row))
                data.SubRegionsTranslations.Add(new()
                {
                    Id = t.Id,
                    SubRegionId = Entity("subregion", row.SourceId),
                    LanguageId = t.LanguageId,
                    Name = t.Name
                });
        IEnumerable<(Guid Id, Guid LanguageId, string Name)> Translations(string kind, GeographyRow row)
        {
            yield return (Id(SeedKeys.Translation(kind, row.SourceId, "en")), en, row.Name);
            // Reserve the Arabic ID even when its label is pending review.
            var arabicId = Id(SeedKeys.Translation(kind, row.SourceId, "ar"));
            if (Arabic(kind, row, corrections) is { } name) yield return (arabicId, ar, name);
        }
        foreach (var row in source.Countries)
        {
            var id = Entity("country", row.SourceId);
            data.Countries.Add(new() { Id = id, CountryName = row.Name });
            foreach (var t in Translations("country", row))
                data.CountriesTranslations.Add(new() { Id = t.Id, CountryId = id, LanguageId = t.LanguageId, Name = t.Name });
        }
        var countryIds = source.Countries.Select(r => r.SourceId).ToHashSet();
        foreach (var row in source.States)
        {
            var id = Entity("state", row.SourceId);
            if (row.CountrySourceId is null || !countryIds.Contains(row.CountrySourceId))
                throw new InvalidDataException($"State {row.SourceId} ({row.Name}) references an unknown country.");
            data.States.Add(new() { Id = id, CountryId = Entity("country", row.CountrySourceId), StateName = row.Name });
            foreach (var t in Translations("state", row))
                data.StatesTranslations.Add(new() { Id = t.Id, StateId = id, LanguageId = t.LanguageId, Name = t.Name });
        }
        var unresolved = new List<SeedIssue>();
        var regionIds = source.Regions.Select(r => r.SourceId).ToHashSet();
        var stateCountries = source.States.ToDictionary(r => r.SourceId, r => r.CountrySourceId);
        foreach (var row in source.Cities)
        {
            var id = Entity("city", row.SourceId);
            var translations = Translations("city", row).ToList();
            if (row.CountrySourceId is null || !countryIds.Contains(row.CountrySourceId)
                || row.RegionSourceId is null || !regionIds.Contains(row.RegionSourceId)
                || row.StateSourceId is null || !stateCountries.TryGetValue(row.StateSourceId, out var country)
                || country != row.CountrySourceId)
            {
                unresolved.Add(new("Relationship", "city", row.SourceId, $"{row.Name}: unresolved country/state/region; retained in source staging."));
                continue;
            }
            data.Cities.Add(new()
            {
                Id = id,
                CityName = row.Name,
                CountryId = Entity("country", row.CountrySourceId),
                StateId = Entity("state", row.StateSourceId),
                RegionId = Entity("region", row.RegionSourceId)
            });
            foreach (var t in translations)
                data.CitiesTranslations.Add(new() { Id = t.Id, CityId = id, LanguageId = t.LanguageId, Name = t.Name });
        }
        foreach (var role in Roles)
        {
            var id = Entity("role", role.Code);
            data.Roles.Add(new() { Id = id });
            data.RoleTranslations.Add(new() { Id = Id(SeedKeys.Translation("role", role.Code, "en")), RoleId = id, LanguageId = en, Name = role.English, Description = role.EnglishDescription });
            data.RoleTranslations.Add(new() { Id = Id(SeedKeys.Translation("role", role.Code, "ar")), RoleId = id, LanguageId = ar, Name = role.Arabic, Description = role.ArabicDescription });
        }
        // Persist identity first, so an interrupted data write never discards assigned IDs.
        await SeedFiles.WriteAsync(mappingPath, ids, token);
        await SeedFiles.WriteAsync(Path.Combine(root, "Staging", "seed-data.json.gz"), data, token);
        var issues = SeedValidator.Validate(data, ids, source, corrections).Concat(unresolved).Distinct().ToList();
        await SeedFiles.WriteAsync(Path.Combine(root, "coverage-report.json"), new
        {
            Complete = issues.Count == 0,
            Counts = new
            {
                Countries = data.Countries.Count,
                States = data.States.Count,
                Cities = data.Cities.Count,
                Roles = data.Roles.Count,
                Regions = data.Regions.Count,
                SubRegions = data.SubRegions.Count,
                RegionsTranslations = data.RegionsTranslations.Count,
                SubRegionsTranslations = data.SubRegionsTranslations.Count
            },
            IssueCounts = issues.GroupBy(i => i.Code).ToDictionary(g => g.Key, g => g.Count()),
            Issues = issues
        }, token);
        return issues;
    }
}
