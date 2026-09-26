using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Countries : BaseAuditLogModel
{
    public required string CountryName { get; set; }
    public ICollection<Users> Users { get; set; } = new List<Users>();
    public ICollection<States> States { get; set; } = new List<States>();
    public ICollection<Cities> Cities { get; set; } = new List<Cities>();
    public ICollection<CountriesTranslation> CountryTranslations { get; set; } = new List<CountriesTranslation>();

}