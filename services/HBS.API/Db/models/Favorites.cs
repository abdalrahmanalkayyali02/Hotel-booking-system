using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Favorites : BaseAuditLogModel
{
    public Guid UserId { get; set; }
    public Guid HotelId { get; set; }
    
    public Users Users { get; set; } = null!;
    public Hotels Hotels { get; set; } = null!;
}
