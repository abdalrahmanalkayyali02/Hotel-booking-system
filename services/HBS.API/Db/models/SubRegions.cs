using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class SubRegions : BaseAuditLogModel
{
    public required string Name { get; set; }
    public Guid RegionId { get; set; }
    public Regions Region { get; set; } = null!;

    public ICollection<SubRegionsTranslation> SubRegionsTranslations { get; set; } = new List<SubRegionsTranslation>();
}