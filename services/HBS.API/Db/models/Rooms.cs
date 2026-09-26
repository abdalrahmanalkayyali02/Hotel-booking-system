using HBS.API.Shared.enums;
using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Rooms : BaseAuditLogModel
{
    public Guid HotelId { get; set; }
    public Guid RoomTypeId { get; set; }
    public required string RoomNumber { get; set; }
    public int? Floor { get; set; }
    public RoomStatus Status { get; set; }

    public Hotels Hotel { get; set; } = null!;
    public RoomTypes RoomType { get; set; } = null!;
    public ICollection<Bookings>  Bookings { get; set; } = new List<Bookings>();

}
