using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Amenities : BaseAuditLogModel
{
    public required string Name { get; set; }
    public required string Icon { get; set; }
    public string? Description { get; set; }

    public ICollection<RoomTypeAmenities> RoomTypeAmenities { get; set; } = new List<RoomTypeAmenities>();
    public ICollection<HotelAmenities>  HotelAmenities { get; set; } = new List<HotelAmenities>();
    public ICollection<AmenitiesTranslation>  AmenityTranslations { get; set; } = new List<AmenitiesTranslation>();
}
