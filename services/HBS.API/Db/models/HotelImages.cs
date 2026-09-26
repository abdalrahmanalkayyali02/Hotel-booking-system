using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class HotelImages : BaseAuditLogModel
{
    public Guid HotelId { get; set; }
    public required string ImageUrl { get; set; }
    public bool IsPrimary { get; set; }

    public Hotels Hotel { get; set; } = null!;
}
