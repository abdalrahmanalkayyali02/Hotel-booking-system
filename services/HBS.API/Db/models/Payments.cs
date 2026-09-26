using HBS.API.Shared.enums;
using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Payments : BaseAuditLogModel
{
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionRef { get; set; }
    public DateTime? PaidAt { get; set; }
    
    public Bookings Booking { get; set; } = null!;
}
