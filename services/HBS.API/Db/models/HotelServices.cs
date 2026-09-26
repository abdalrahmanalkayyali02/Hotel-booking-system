using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class HotelServices : BaseAuditLogModel
{
    public Guid HotelId { get; set; }
    public Guid ServiceId { get; set; }
    public Hotels Hotels { get; set; } = null!;
    public Services Services { get; set; } = null!;
}
