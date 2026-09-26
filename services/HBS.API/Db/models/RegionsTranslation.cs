using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class RegionsTranslation : BaseAuditLogModel
{
    public Guid RegionId { get; set; }
    public Regions Region { get; set; } = null!;
    public Guid LanguageId { get; set; }
    public Languages Language { get; set; } = null!;
    public required string Name { get; set; }
}