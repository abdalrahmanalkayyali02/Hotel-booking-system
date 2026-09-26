using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class ServicesTranslation : BaseAuditLogModel
{
    public Guid ServiceId { get; set; }
    public Guid LanguageId { get; set; }
    public required string Name  { get; set; }
    public string? Description { get; set; }

    public Services Service { get; set; } = null!;
    public Languages Language { get; set; } = null!;
}