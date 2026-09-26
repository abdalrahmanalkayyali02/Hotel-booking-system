using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Regions : BaseAuditLogModel
{
    public required string RegionName { get; set; }

    public ICollection<Cities> Cities { get; set; } = new List<Cities>();
    public ICollection<RegionsTranslation> RegionsTranslations { get; set; } = new List<RegionsTranslation>();
    public ICollection<SubRegions> SubRegions { get; set; } = new List<SubRegions>();
}