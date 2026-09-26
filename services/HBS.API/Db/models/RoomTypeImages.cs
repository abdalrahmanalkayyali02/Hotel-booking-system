using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class RoomTypeImages : BaseAuditLogModel
{
    public Guid RoomTypeId { get; set; }
    public required string ImageUrl { get; set; }
    public RoomTypes RoomType { get; set; } = null!;
}
