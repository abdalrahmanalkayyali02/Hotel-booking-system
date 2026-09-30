using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Roles : BaseAuditLogModel
{
    public required string Code { get; set; } //used for authorization
    public ICollection<RoleTranslation> RoleTranslations { get; set; } = new List<RoleTranslation>();
    public ICollection<Users> Users { get; set; } = new List<Users>();
    public ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();

}
