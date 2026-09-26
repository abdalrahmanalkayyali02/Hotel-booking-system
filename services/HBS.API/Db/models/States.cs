using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class States : BaseAuditLogModel
{
    public Guid CountryId { get; set; }
    public Countries Country { get; set; } = null!;
    public required string StateName { get; set; }
    public ICollection<StatesTranslation> StatesTranslations { get; set; } = new List<StatesTranslation>();
    public ICollection<Cities> Cities { get; set; } = new List<Cities>();

}