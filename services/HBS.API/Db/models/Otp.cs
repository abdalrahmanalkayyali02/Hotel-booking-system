using HBS.API.Shared.enums;
using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Otp : BaseAuditLogModel
{
    public required string HashedOtp { get; set; }
    public required OtpType Type { get; set; }
    public required OtpTarget Target { get; set; }
    public Guid UserId { get; set; }
    public Users User { get; set; } = null!;
    public required DateTime GeneratedAt { get; set; }
    public required DateTime ExpiresAt { get; set; }
    public required bool IsUsed { get; set; }
    public required int NumberOfAttempts { get; set; }
}