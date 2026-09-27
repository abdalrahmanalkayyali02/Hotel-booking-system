using HBS.API.Shared.enums;

namespace HBS.API.Dtos.Hotels.UpdateHotel;

public record UpdateHotelRequest(
  float StarRating,
  string PhoneNumberCountryCode,
  string PhoneNumber,
  string? Email,
  List<UpdateHotelTranslationRequest> Translations
);

public record UpdateHotelResponse(
  Guid HotelId,
  HotelStatus Status
);
