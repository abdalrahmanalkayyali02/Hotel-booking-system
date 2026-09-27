using HBS.API.Shared.enums;

namespace HBS.API.Dtos.Hotels.CreateHotel;

public record CreateHotelRequest(
    Guid? ManagerId,
    string City,
    string Country,
    float StarRating,
    string? PhoneNumberCountryCode,
    string? PhoneNumber,
    string? Email,
    List<CreateHotelTranslationRequest> Translations
);

public record CreateHotelResponse(
    Guid HotelId,
    HotelStatus Status
);
