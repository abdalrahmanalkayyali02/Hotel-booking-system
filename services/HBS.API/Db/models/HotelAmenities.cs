using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class HotelAmenities : BaseAuditLogModel
{
    public Guid HotelId { get; set; }
    public Guid AmenityId { get; set; }
    public Hotels Hotel { get; set; } = null!;
    public Amenities Amenity { get; set; } = null!;
}
