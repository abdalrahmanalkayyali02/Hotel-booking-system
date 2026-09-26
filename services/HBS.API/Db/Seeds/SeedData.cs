using HBS.API.Db.models;

namespace HBS.API.Db.Seeds;

public sealed class SeedData
{
    public List<Languages> Languages { get; set; } = [];
    public List<Regions> Regions { get; set; } = [];
    public List<SubRegions> SubRegions { get; set; } = [];
    public List<RegionsTranslation> RegionsTranslations { get; set; } = [];
    public List<SubRegionsTranslation> SubRegionsTranslations { get; set; } = [];
    public List<Countries> Countries { get; set; } = [];
    public List<States> States { get; set; } = [];
    public List<Cities> Cities { get; set; } = [];
    public List<Roles> Roles { get; set; } = [];
    public List<CountriesTranslation> CountriesTranslations { get; set; } = [];
    public List<StatesTranslation> StatesTranslations { get; set; } = [];
    public List<CitiesTranslation> CitiesTranslations { get; set; } = [];
    public List<RoleTranslation> RoleTranslations { get; set; } = [];
}

public sealed record SeedIssue(string Code, string Entity, string SourceId, string Message);

public sealed class SeedValidationException(IReadOnlyList<SeedIssue> issues)
    : Exception($"Seed validation failed with {issues.Count} issue(s). First: {issues.FirstOrDefault()}")
{
    public IReadOnlyList<SeedIssue> Issues { get; } = issues;
}

public sealed class GeographySource
{
    public List<GeographyRow> Regions { get; set; } = [];
    public List<GeographyRow> SubRegions { get; set; } = [];
    public List<GeographyRow> Countries { get; set; } = [];
    public List<GeographyRow> States { get; set; } = [];
    public List<GeographyRow> Cities { get; set; } = [];
}

public sealed record GeographyRow
{
    public required string SourceId { get; init; }
    public required string Name { get; init; }
    public string? ArabicName { get; init; }
    public string? CountrySourceId { get; init; }
    public string? RegionSourceId { get; init; }
    public string? StateSourceId { get; init; }
}

public sealed record TranslationCorrection(string Name, string SourceUrl, string ReviewedBy,
    string SourceType = "reference", string? GenerationMethod = null, string? Verification = null);

public static class SeedKeys
{
    public static readonly string[] RoleCodes = ["super_admin", "hotel_admin", "receptionist", "customer", "guest"];
    public static string Entity(string kind, string sourceId) => $"{kind}/{sourceId}";
    public static string Translation(string kind, string sourceId, string language) => $"{kind}-translation/{sourceId}/{language}";
}
