using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class PermissionsTranslation : BaseAuditLogModel
{
    public Guid PermissionId { get; set; }
    public Permissions Permission { get; set; } = null!;
    public Guid LanguageId { get; set; }
    public Languages Language { get; set; } = null!;
    public required string Name { get; set; }
    public string? Description { get; set; }
}

