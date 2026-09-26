using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Notifications : BaseAuditLogModel
{
    public Guid UserId { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public required string Type { get; set; }
    public bool IsRead { get; set; } = false;
    public Users User { get; set; } = null!;
}
