using HBS.API.Shared.enums;
using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class HotelRequests : BaseAuditLogModel
{
    public Guid UserId { get; set; }
    public Guid HotelId { get; set; }
    public HotelRequestStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public Users User { get; set; } = null!;
    public Hotels Hotel { get; set; } = null!;
}
