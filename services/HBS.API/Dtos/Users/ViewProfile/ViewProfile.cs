using HBS.API.Shared.enums;

namespace HBS.API.Dtos.Users.ViewProfile;

public record ViewProfileResponse(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumberCountryCode,
    string? PhoneNumber,
    DateOnly BirthDate,
    string RoleName,
    string StatusName,
    string CountryName,
    string CityName
);