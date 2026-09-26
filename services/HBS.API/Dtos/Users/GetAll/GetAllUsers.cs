using HBS.API.Shared.enums;

namespace HBS.API.Dtos.Users.GetAll;

public record GetAllUsersResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumberCountryCode,
    string? PhoneNumber,
    DateOnly BirthDate,
    bool PhoneNumberConfirmed,
    bool EmailConfirmed,
    Guid RoleId,
    UserStatus Status,
    Guid CountryId,
    Guid CityId
);