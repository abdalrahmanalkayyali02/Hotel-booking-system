using HBS.API.Shared.enums;
using HBS.API.Shared.Models;

namespace HBS.API.Db.models;

public class Users : BaseAuditLogModel
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required DateOnly BirthDate { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public required string PasswordHash { get; set; }
    public string? PhoneNumberCountryCode { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public Guid RoleId { get; set; }
    public Roles Role { get; set; } = null!;
    public required UserStatus Status { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid CountryId { get; set; }
    public Countries Country { get; set; } = null!;
    public Guid CityId { get; set; }
    public Cities City { get; set; } = null!;
    public ICollection<Otp> Otps { get; set; } = new List<Otp>();
    public ICollection<Notifications>  Notifications { get; set; } = new List<Notifications>();
    public ICollection<Hotels> ManagedHotels { get; set; } = new List<Hotels>();
    public ICollection<HotelRequests>  HotelRequests { get; set; } = new List<HotelRequests>();
    public ICollection<Bookings>  Bookings { get; set; } = new List<Bookings>();
    public ICollection<Reviews>  Reviews { get; set; } = new List<Reviews>();
    public ICollection<Favorites> Favorites  { get; set; } = new List<Favorites>();
    
}