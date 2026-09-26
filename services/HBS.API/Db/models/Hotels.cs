using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Hotels : BaseAuditLogModel
{
    public required string City { get; set; }
    public required string Country { get; set; }
    public required float StarRating { get; set; }
    public string? PhoneNumberCountryCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public Guid ManagerId { get; set; }
    public Users Manager { get; set; } = null!;
    public ICollection<HotelRequests>  HotelRequests { get; set; } = new List<HotelRequests>();
    public ICollection<HotelImages> HotelImages { get; set; } = new List<HotelImages>();
    public ICollection<HotelAmenities> HotelAmenities { get; set; } = new List<HotelAmenities>();
    public ICollection<HotelServices> HotelServices { get; set; } = new List<HotelServices>();
    public ICollection<RoomTypes> RoomTypes { get; set; } = new List<RoomTypes>();
    public ICollection<Rooms> Rooms { get; set; } = new List<Rooms>();
    public ICollection<Bookings> Bookings { get; set; } = new List<Bookings>();
    public ICollection<Reviews> Reviews { get; set; } = new List<Reviews>();
    public ICollection<Favorites> Favorites { get; set; } = new List<Favorites>();
    public ICollection<HotelsTranslation> Translations { get; set; } = new List<HotelsTranslation>();
    
}
