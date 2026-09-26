using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class HotelsTranslation : BaseAuditLogModel
{
    public Guid HotelId { get; set; }
    public Guid LanguageId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Address { get; set; }

    public Hotels Hotels { get; set; } = null!;
    public Languages Languages { get; set; } = null!;
}