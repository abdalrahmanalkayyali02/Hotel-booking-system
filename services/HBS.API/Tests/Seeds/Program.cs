using System.Security.Cryptography;
using System.Text.Json;
using HBS.API.Db.Seeds;

var root = Path.Combine(Path.GetTempPath(), "hbs-seed-tests-" + Guid.NewGuid());
Directory.CreateDirectory(root);
var passed = 0;
void Assert(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    passed++;
}
async Task Throws<T>(Func<Task> action, string message) where T : Exception
{
    try { await action(); }
    catch (T) { passed++; return; }
    throw new Exception(message);
}
try
{
    var source = new GeographySource
    {
        Regions = [new() { SourceId = "3", Name = "Asia", ArabicName = "آسيا" }, new() { SourceId = "4", Name = "Europe", ArabicName = "أوروبا" }],
        SubRegions = [new() { SourceId = "14", Name = "Southern Asia", ArabicName = "جنوب آسيا", RegionSourceId = "3" }],
        Countries = [new() { SourceId = "111", Name = "Jordan", ArabicName = "الأردن", CountrySourceId = "111", RegionSourceId = "3" }],
        States = [new() { SourceId = "963", Name = "Amman", ArabicName = "عمّان", CountrySourceId = "111", RegionSourceId = "3" }],
        Cities = [new() { SourceId = "63171", Name = "Amman", ArabicName = "عمّان", CountrySourceId = "111", RegionSourceId = "3", StateSourceId = "963" }]
    };
    async Task SaveSource()
    {
        await SeedFiles.WriteAsync(Path.Combine(root, "Sources/geography.json.gz"), source);
        var hash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(root, "Sources/geography.json.gz"))));
        await SeedFiles.WriteAsync(Path.Combine(root, "Sources/manifest.json"), new { SnapshotSha256 = hash, BaselineSha256 = new Dictionary<string, string>() });
    }
    await SaveSource();
    await SeedFiles.WriteAsync(Path.Combine(root, "Sources/arabic-corrections.json"), new Dictionary<string, TranslationCorrection>());
    Assert((await SeedGenerator.GenerateAsync(root)).Count == 0, "Complete fixture should generate without issues.");
    var loader = new SeedDataLoader();
    var data = await loader.LoadAsync(root);
    var ids = await SeedFiles.ReadAsync<SortedDictionary<string, Guid>>(Path.Combine(root, "id-map.json.gz"));
    Assert(data.States[0].CountryId == data.Countries[0].Id, "State must reference its source country.");
    Assert(data.SubRegions.Single().RegionId == data.Regions[0].RegionId, "Subregion must reference its CSV region.");
    Assert(data.RegionsTranslations.Count == 4 && data.SubRegionsTranslations.Count == 2, "Regions and subregions must be bilingual.");
    Assert(data.RegionsTranslations.Count(t => t.LanguageId == ids["language/ar"]) == 2
        && data.SubRegionsTranslations.Count(t => t.LanguageId == ids["language/ar"]) == 1, "Arabic region coverage is required.");
    Assert(ids.ContainsKey("region-translation/3/ar") && ids.ContainsKey("subregion-translation/14/ar"), "Persist Arabic region/subregion mappings.");
    Assert(ids.Values.All(SeedValidator.IsUuid7), "Every identity must be UUIDv7.");
    Assert(ids.Count == ids.Values.Distinct().Count(), "Identities must be globally unique.");
    Assert(data.Roles.Count == 5 && data.RoleTranslations.Count == 10, "Exactly five bilingual roles required.");
    Assert(data.CountriesTranslations.Any(t => t.Name == "الأردن") && data.CitiesTranslations.Any(t => t.Name == "عمّان"), "Arabic must round-trip.");
    Assert(data.RoleTranslations.Select(t => t.Name).Contains("موظف الاستقبال"), "Receptionist Arabic name missing.");
    var mapBefore = await File.ReadAllBytesAsync(Path.Combine(root, "id-map.json.gz"));
    var dataBefore = await File.ReadAllBytesAsync(Path.Combine(root, "Staging/seed-data.json.gz"));
    await SeedGenerator.GenerateAsync(root);
    var mapAfter = await File.ReadAllBytesAsync(Path.Combine(root, "id-map.json.gz"));
    var dataAfter = await File.ReadAllBytesAsync(Path.Combine(root, "Staging/seed-data.json.gz"));
    Assert(mapBefore.SequenceEqual(mapAfter), "Regeneration changed IDs.");
    Assert(dataBefore.SequenceEqual(dataAfter), "Regeneration changed data.");
    passed += await ImportTests.RunAsync(root, data);
    var baseline = JsonSerializer.Serialize(data);
    void Reject(Action<SeedData> change, string code)
    {
        var modified = JsonSerializer.Deserialize<SeedData>(baseline)!;
        change(modified);
        Assert(SeedValidator.Validate(modified, ids, source, new Dictionary<string, TranslationCorrection>()).Any(i => i.Code == code), $"Expected {code}.");
    }
    Reject(d => d.States[0].CountryId = Guid.Empty, "ForeignKey");
    Reject(d => d.States[0].CountryId = Guid.CreateVersion7(), "Relationship");
    Reject(d => d.Cities[0].CountryId = Guid.CreateVersion7(), "ForeignKey");
    Reject(d => d.Cities[0].StateId = d.Countries[0].Id, "Relationship");
    Reject(d => d.Countries[0].Id = Guid.NewGuid(), "UuidVersion");
    Reject(d => d.CitiesTranslations.Add(d.CitiesTranslations[0]), "DuplicateTranslation");
    Reject(d => d.CitiesTranslations[0].Id = d.Countries[0].Id, "DuplicateId");
    Reject(d => d.CitiesTranslations.RemoveAt(0), "MissingTranslation");
    Reject(d => d.CitiesTranslations[0].LanguageId = Guid.CreateVersion7(), "ForeignKey");
    Reject(d => d.CitiesTranslations[1].Name = "Amman", "UnverifiedTranslation");
    Reject(d => d.RoleTranslations[0].Description = "", "RequiredDescription");
    Reject(d => d.Cities.Clear(), "MissingEntity");
    Reject(d => d.CitiesTranslations[0].Name = "", "RequiredName");
    Reject(d => d.States = null!, "MalformedData");
    Reject(d => d.Cities.Add(null!), "MalformedData");
    Reject(d => d.SubRegions[0].RegionId = Guid.Empty, "ForeignKey");
    Reject(d => d.SubRegions[0].RegionId = d.Regions[1].RegionId, "Relationship");
    Reject(d => d.SubRegions[0].Name = "", "NameMismatch");
    Reject(d => d.SubRegions[0].Name = new string('x', 51), "MaxLength");
    Reject(d => d.Regions[0].RegionName = new string('x', 51), "MaxLength");
    Reject(d => d.SubRegions[0].Id = d.Regions[0].RegionId, "DuplicateId");
    Reject(d => d.SubRegions.Clear(), "MissingEntity");
    Reject(d => d.SubRegions = null!, "MalformedData");
    Reject(d => d.RegionsTranslations[0].RegionId = Guid.Empty, "ForeignKey");
    Reject(d => d.SubRegionsTranslations[0].SubRegionId = Guid.Empty, "ForeignKey");
    Reject(d => d.RegionsTranslations[0].LanguageId = Guid.CreateVersion7(), "ForeignKey");
    Reject(d => d.SubRegionsTranslations[0].LanguageId = Guid.CreateVersion7(), "ForeignKey");
    Reject(d => d.RegionsTranslations[0].LanguageId = Guid.CreateVersion7(), "UnexpectedTranslation");
    Reject(d => d.SubRegionsTranslations[0].LanguageId = Guid.CreateVersion7(), "UnexpectedTranslation");
    Reject(d => d.RegionsTranslations.Add(d.RegionsTranslations[0]), "DuplicateTranslation");
    Reject(d => d.SubRegionsTranslations.Add(d.SubRegionsTranslations[0]), "DuplicateTranslation");
    Reject(d => d.RegionsTranslations[0].Id = d.SubRegions[0].Id, "DuplicateId");
    Reject(d => d.SubRegionsTranslations[0].Id = Guid.CreateVersion7(), "MappingMismatch");
    Reject(d => d.RegionsTranslations.Clear(), "MissingTranslation");
    Reject(d => d.SubRegionsTranslations.Clear(), "MissingTranslation");
    Reject(d => d.RegionsTranslations[0].Name = "", "RequiredName");
    Reject(d => d.SubRegionsTranslations[0].Name = null!, "RequiredName");
    Reject(d => d.RegionsTranslations[0].Name = new string('x', 51), "MaxLength");
    Reject(d => d.SubRegionsTranslations[0].Name = new string('x', 51), "MaxLength");
    Reject(d => d.RegionsTranslations.RemoveAll(t => t.LanguageId == ids["language/ar"]), "MissingTranslation");
    Reject(d => d.SubRegionsTranslations.RemoveAll(t => t.LanguageId == ids["language/ar"]), "MissingTranslation");
    Reject(d => d.RegionsTranslations[1].Name = "Asia", "ArabicScript");
    Reject(d => d.SubRegionsTranslations[1].Name = "Юг", "ArabicScript");
    Reject(d => d.CitiesTranslations[1].Name = new string('ا', 101), "MaxLength");
    Reject(d => d.StatesTranslations[1].Name = new string('ا', 51), "MaxLength");
    var brokenMap = new SortedDictionary<string, Guid>(ids);
    brokenMap.Remove("city/63171");
    Assert(SeedValidator.Validate(data, brokenMap, source, new Dictionary<string, TranslationCorrection>()).Any(i => i.Code == "MissingMapping"), "Missing identity must fail.");

    var originalState = source.States[0];
    foreach (var countrySourceId in new string?[] { null, "unknown-country" })
    {
        source.States[0] = originalState with { CountrySourceId = countrySourceId };
        await SaveSource();
        await Throws<InvalidDataException>(async () => { await SeedGenerator.GenerateAsync(root); }, "Invalid state country accepted.");
        var stagingAfterFailure = await File.ReadAllBytesAsync(Path.Combine(root, "Staging/seed-data.json.gz"));
        Assert(dataBefore.SequenceEqual(stagingAfterFailure), "Invalid source overwrote staging data.");
    }
    source.States[0] = originalState;

    var originalSubRegion = source.SubRegions[0];
    foreach (var invalid in new[]
    {
        originalSubRegion with { RegionSourceId = null },
        originalSubRegion with { RegionSourceId = "unknown-region" },
        originalSubRegion with { Name = "" },
        originalSubRegion with { Name = new string('x', 51) }
    })
    {
        source.SubRegions[0] = invalid;
        await SaveSource();
        await Throws<InvalidDataException>(async () => { await SeedGenerator.GenerateAsync(root); }, "Invalid subregion source accepted.");
        var stagedAfterFailure = await File.ReadAllBytesAsync(Path.Combine(root, "Staging/seed-data.json.gz"));
        var mappingAfterFailure = await File.ReadAllBytesAsync(Path.Combine(root, "id-map.json.gz"));
        Assert(dataBefore.SequenceEqual(stagedAfterFailure), "Invalid subregion source changed staging.");
        Assert(mapBefore.SequenceEqual(mappingAfterFailure), "Invalid subregion source changed identity mapping.");
    }
    source.SubRegions[0] = originalSubRegion;
    source.SubRegions.Add(originalSubRegion);
    await SaveSource();
    await Throws<InvalidDataException>(async () => { await SeedGenerator.GenerateAsync(root); }, "Duplicate subregion CSV ID accepted.");
    source.SubRegions.RemoveAt(1);

    await SeedFiles.WriteAsync(Path.Combine(root, "Sources/arabic-corrections.json"), new Dictionary<string, TranslationCorrection>
    {
        ["city/63171"] = new("اسم مختلف", "https://example.org/source", "Test fixture", "generated", "Test generation")
    });
    await Throws<InvalidDataException>(async () => { await SeedGenerator.GenerateAsync(root); }, "Accepted source Arabic was overwritten.");
    await SeedFiles.WriteAsync(Path.Combine(root, "Sources/arabic-corrections.json"), new Dictionary<string, TranslationCorrection>());
    var originalRegion = source.Regions[0];
    source.Regions[0] = originalRegion with { ArabicName = null };
    source.SubRegions[0] = originalSubRegion with { ArabicName = null };
    await SaveSource();
    var regionGaps = await SeedGenerator.GenerateAsync(root);
    Assert(regionGaps.Count(i => i.Code == "MissingArabic" && i.Entity is "region" or "subregion") == 2, "English-only region sources must fail bilingual validation.");
    await Throws<SeedValidationException>(async () => { await loader.LoadAsync(root); }, "Missing Arabic region coverage was accepted.");
    var regionCorrections = new Dictionary<string, TranslationCorrection>
    {
        ["region/3"] = new("آسيا", "https://example.org/english-source", "Test fixture", "generated", "Test generation"),
        ["subregion/14"] = new("جنوب آسيا", "https://example.org/english-source", "Test fixture", "generated", "Test generation")
    };
    await SeedFiles.WriteAsync(Path.Combine(root, "Sources/arabic-corrections.json"), regionCorrections);
    Assert((await SeedGenerator.GenerateAsync(root)).Count == 0, "Generated region corrections must fill both gaps.");
    var correctedRegions = await loader.LoadAsync(root);
    Assert(correctedRegions.RegionsTranslations.Any(t => t.Id == ids["region-translation/3/ar"] && t.Name == "آسيا")
        && correctedRegions.SubRegionsTranslations.Any(t => t.Id == ids["subregion-translation/14/ar"] && t.Name == "جنوب آسيا"), "Region corrections must reuse reserved IDs.");
    regionCorrections["region/3"] = regionCorrections["region/3"] with { GenerationMethod = null };
    await SeedFiles.WriteAsync(Path.Combine(root, "Sources/arabic-corrections.json"), regionCorrections);
    await Throws<InvalidDataException>(async () => { await SeedGenerator.GenerateAsync(root); }, "Generated correction without generation provenance accepted.");
    source.Regions[0] = originalRegion;
    source.SubRegions[0] = originalSubRegion;
    await SeedFiles.WriteAsync(Path.Combine(root, "Sources/arabic-corrections.json"), new Dictionary<string, TranslationCorrection>());

    source.Cities[0] = source.Cities[0] with { ArabicName = null };
    await SaveSource();
    Assert((await SeedGenerator.GenerateAsync(root)).Any(i => i.Code == "MissingArabic"), "Missing Arabic must be reported.");
    await Throws<SeedValidationException>(async () => { await loader.LoadAsync(root); }, "Strict loader accepted incomplete Arabic.");
    var reservedArabicId = ids["city-translation/63171/ar"];
    await SeedFiles.WriteAsync(Path.Combine(root, "Sources/arabic-corrections.json"), new Dictionary<string, TranslationCorrection>
    {
        ["city/63171"] = new("عمّان", "https://example.org/reviewed-test-fixture", "Test fixture")
    });
    Assert((await SeedGenerator.GenerateAsync(root)).Count == 0, "Reviewed correction should fill gap.");
    Assert((await loader.LoadAsync(root)).CitiesTranslations.Any(t => t.Id == reservedArabicId && t.Name == "عمّان"), "Correction changed reserved ID.");

    var staging = Path.Combine(root, "Staging/seed-data.json.gz");
    await SeedFiles.WriteAsync(staging, new { Cities = new[] { new { Id = "invalid-guid" } } });
    await Throws<JsonException>(async () => { await loader.LoadAsync(root); }, "Malformed data accepted.");
    await SeedGenerator.GenerateAsync(root);
    File.Move(Path.Combine(root, "id-map.json.gz"), Path.Combine(root, "id-map.backup.gz"));
    await Throws<FileNotFoundException>(async () => { await loader.LoadAsync(root); }, "Missing file accepted.");
    await Throws<InvalidDataException>(async () => { await SeedGenerator.GenerateAsync(root); }, "Lost identity mapping regenerated silently.");
    if (args.Length == 1)
    {
        var actualRoot = Path.GetFullPath(args[0]);
        var (actual, actualIssues) = await loader.AuditAsync(actualRoot);
        Assert(actual.Countries.Count == 250 && actual.States.Count == 5308 && actual.Cities.Count == 152970, "Worldwide source coverage changed.");
        Assert(actual.Regions.Count == 6 && actual.SubRegions.Count == 22
            && actual.RegionsTranslations.Count == 12 && actual.SubRegionsTranslations.Count == 44, "Bilingual geography coverage is incomplete.");
        Assert(actualIssues.Count == 0, "Worldwide data must pass every strict validation check.");
        await loader.LoadAsync(actualRoot);
        var actualIds = await SeedFiles.ReadAsync<SortedDictionary<string, Guid>>(Path.Combine(actualRoot, "id-map.json.gz"));
        var arabicId = actualIds["language/ar"];
        Assert(actual.CountriesTranslations.Count(t => t.LanguageId == arabicId) == 250
            && actual.StatesTranslations.Count(t => t.LanguageId == arabicId) == 5308
            && actual.CitiesTranslations.Count(t => t.LanguageId == arabicId) == 152970
            && actual.RegionsTranslations.Count(t => t.LanguageId == arabicId) == 6
            && actual.SubRegionsTranslations.Count(t => t.LanguageId == arabicId) == 22, "All five geography levels require Arabic coverage.");
        Assert(actualIds.Count == 475685, "Unexpected persistent identity count.");
        var actualMapPath = Path.Combine(actualRoot, "id-map.json.gz");
        var actualDataPath = Path.Combine(actualRoot, "Staging/seed-data.json.gz");
        var originalMap = SHA256.HashData(await File.ReadAllBytesAsync(actualMapPath));
        var originalData = SHA256.HashData(await File.ReadAllBytesAsync(actualDataPath));
        await SeedGenerator.GenerateAsync(actualRoot);
        var regeneratedMap = SHA256.HashData(await File.ReadAllBytesAsync(actualMapPath));
        var regeneratedData = SHA256.HashData(await File.ReadAllBytesAsync(actualDataPath));
        Assert(originalMap.SequenceEqual(regeneratedMap), "Worldwide UUID mappings changed during regeneration.");
        Assert(originalData.SequenceEqual(regeneratedData), "Worldwide model data changed during regeneration.");
    }
    if (args.Length == 2 && args[0] == "--import-audit")
    {
        // Read staging only, then exercise the import plan against an in-memory fake database.
        // This path never regenerates the real seed artifacts or opens a database connection.
        passed += await ImportTests.CheckWorldwideAsync(await loader.LoadAsync(Path.GetFullPath(args[1])));
    }
    Console.WriteLine($"PASS: {passed} seed checks.");
}
finally
{
    Directory.Delete(root, recursive: true);
}
