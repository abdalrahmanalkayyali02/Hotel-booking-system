using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class RoomTypesTranslation : BaseAuditLogModel
{
    public Guid RoomTypeId { get; set; }
    public Guid LanguageId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string BedType { get; set; }

    public RoomTypes RoomType { get; set; } = null!;
    public Languages Language { get; set; } = null!;
}