using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class CountriesTranslation : BaseAuditLogModel
{
    public Guid CountryId { get; set; }
    public Countries Country { get; set; } = null!;
    public Guid LanguageId { get; set; }
    public Languages Language { get; set; } = null!;
    public required string Name { get; set; }
}
