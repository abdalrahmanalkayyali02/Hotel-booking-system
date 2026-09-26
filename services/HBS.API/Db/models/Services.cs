using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Services : BaseAuditLogModel
{
    public required string Icon  { get; set; }
    public ICollection<HotelServices>  HotelServices { get; set; } = new List<HotelServices>();
    public ICollection<ServicesTranslation> ServicesTranslations { get; set; } = new List<ServicesTranslation>();
}
