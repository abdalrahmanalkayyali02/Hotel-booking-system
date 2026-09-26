using HBS.API.Shared.enums;

namespace HBS.API.Dtos.Users.Registeration;

public record RegisterStandardUsersDtos(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumberCountryCode,
    string PhoneNumberValue,
    DateOnly BirthDate,
    Guid CountryId,
    Guid CityId);

public record RegisterStandardUsersResponse(
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
    Guid CityId
);

