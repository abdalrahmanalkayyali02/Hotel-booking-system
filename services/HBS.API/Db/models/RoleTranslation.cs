using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class RoleTranslation : BaseAuditLogModel
{
    public Guid RoleId { get; set; }
    public Roles Role { get; set; } = null!;
    public Guid LanguageId { get; set; }
    public Languages Language { get; set; } = null!;
    public required string Name { get; set; }
    public required string Description { get; set; }
}

