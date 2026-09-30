using HBS.API.Dtos.Hotels.CreateHotel;
using HBS.API.Dtos.Users.Registeration;
using HBS.API.Shared.enums;

namespace HBS.API.Dtos.Users.RegisterHotelAdmin;

public record RegisterHotelAdminRequest(
    RegisterStandardUsersDtos User,
    CreateHotelRequest Hotel
);

public record RegisterHotelAdminResponse(
  Guid Id,
  string FirstName,
  string LastName,
  string Email,
  string PhoneNumberCountryCode,
  string PhoneNumber,
  DateOnly BirthDate,
  bool PhoneNumberConfirmed,
  bool EmailConfirmed,
  Guid RoleId,
  UserStatus Status,
  Guid CountryId,
  Guid CityId,
  Guid HotelId,
  HotelStatus HotelStatus
);
