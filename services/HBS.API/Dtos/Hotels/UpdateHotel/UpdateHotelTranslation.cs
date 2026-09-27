namespace HBS.API.Dtos.Hotels.UpdateHotel;

public record UpdateHotelTranslationRequest(
  string LanguageCode,
  string Name,
  string? Description,
  string Address
  );
