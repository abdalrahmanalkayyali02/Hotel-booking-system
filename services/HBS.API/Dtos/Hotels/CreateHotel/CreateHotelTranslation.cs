namespace HBS.API.Dtos.Hotels.CreateHotel;

public record CreateHotelTranslationRequest(
    string LanguageCode,
    string Name,
    string? Description,
    string Address
);