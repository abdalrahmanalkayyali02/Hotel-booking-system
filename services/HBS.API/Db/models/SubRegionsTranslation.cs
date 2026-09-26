using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class SubRegionsTranslation : BaseAuditLogModel
{
    public Guid SubRegionId { get; set; }
    public SubRegions SubRegion { get; set; } = null!;
    public Guid LanguageId { get; set; }
    public Languages Language { get; set; } = null!;
    public required string Name { get; set; }
}