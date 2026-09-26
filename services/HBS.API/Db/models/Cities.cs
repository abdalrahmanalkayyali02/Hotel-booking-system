using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Cities : BaseAuditLogModel
{
    public Guid CountryId { get; set; }
    public Countries Country { get; set; } = null!;
    public Guid RegionId { get; set; }
    public Regions Region { get; set; } = null!;
    public Guid StateId { get; set; }
    public States State { get; set; } = null!;
    public required string CityName { get; set; }

    public ICollection<CitiesTranslation> CitiesTranslations { get; set; } = new List<CitiesTranslation>();
    public ICollection<Users> Users { get; set; } = new List<Users>();

}