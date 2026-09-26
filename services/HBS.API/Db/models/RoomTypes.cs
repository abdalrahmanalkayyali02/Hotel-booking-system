using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class RoomTypes : BaseAuditLogModel
{
    public Guid HotelId { get; set; }
    public decimal BasePrice { get; set; }
    public byte MaxOccupancy { get; set; }

    public Hotels Hotel { get; set; } = null!;
    public ICollection<Rooms> Rooms { get; set; } = new List<Rooms>();
    public ICollection<RoomTypeAmenities> RoomTypeAmenities { get; set; } = new List<RoomTypeAmenities>();
    public ICollection<RoomTypeImages> RoomTypeImages { get; set; } = new List<RoomTypeImages>();
    public ICollection<Bookings>  Bookings { get; set; } = new List<Bookings>();
    public ICollection<RoomTypesTranslation>  RoomTypeTranslations { get; set; } = new List<RoomTypesTranslation>();
    
}
