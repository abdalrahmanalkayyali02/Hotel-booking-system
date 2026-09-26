using HBS.API.Shared.enums;
using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Bookings : BaseAuditLogModel
{
    public required string BookingReference { get; set; }
    public Guid UserId { get; set; }
    public Guid HotelId { get; set; }
    public Guid RoomTypeId { get; set; }
    public Guid? RoomId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public byte NumberOfGuests { get; set; }
    public BookingStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid? PromotionId { get; set; }
    
    public Users User { get; set; } = null!;
    public Hotels Hotel { get; set; } = null!;
    public RoomTypes RoomType { get; set; } = null!;
    public Rooms? Room { get; set; }
    public ICollection<Payments> Payments { get; set; } = new List<Payments>();
    public Reviews? Review { get; set; }
}
