using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class AmenitiesTranslation : BaseAuditLogModel
{
    public Guid AmenityId { get; set; }
    public Guid LanguageId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public Amenities Amenity { get; set; } = null!;
    public Languages Language { get; set; } = null!;
}