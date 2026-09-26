namespace HBS.API.Dtos.Users.Update;

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string PhoneNumberCountryCode,
    string PhoneNumberValue,
    DateOnly BirthDate,
    Guid CountryId,
    Guid CityId
);

public record UpdateUserResponse(
    string FirstName,
    string LastName,
    string PhoneNumberCountryCode,
    string PhoneNumberValue,
    DateOnly BirthDate,
    Guid CountryId,
    Guid CityId
);

