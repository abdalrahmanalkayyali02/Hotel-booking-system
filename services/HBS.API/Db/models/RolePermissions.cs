using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class RolePermissions : BaseAuditLogModel
{
    public Guid RoleId { get; set; }
    public Roles Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permissions Permission { get; set; } = null!;

}