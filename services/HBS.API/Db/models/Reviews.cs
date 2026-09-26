using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Reviews : BaseAuditLogModel
{
    public Guid HotelId { get; set; }
    public Guid UserId { get; set; }
    public Guid? BookingId { get; set; }
    public byte Rating { get; set; }
    public string? Comment { get; set; }
    
    public Hotels Hotel { get; set; } = null!;
    public Users User { get; set; } = null!;
    public Bookings? Booking { get; set; }
}
