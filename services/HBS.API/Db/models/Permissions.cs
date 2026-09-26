using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Permissions : BaseAuditLogModel
{
    public required string Code { get; set; }

    public ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
    public ICollection<PermissionsTranslation> PermissionTranslations { get; set; } = new List<PermissionsTranslation>();
}