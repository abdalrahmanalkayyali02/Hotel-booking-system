using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class RoomTypeAmenities : BaseAuditLogModel
{
    public Guid RoomTypeId { get; set; }
    public Guid AmenityId { get; set; }
    public RoomTypes RoomType { get; set; } = null!;
    public Amenities Amenity { get; set; } = null!;
}
