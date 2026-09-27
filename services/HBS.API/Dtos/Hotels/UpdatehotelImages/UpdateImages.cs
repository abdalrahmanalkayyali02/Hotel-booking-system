namespace HBS.API.Dtos.Hotels.UpdateHotelImages;

public record UpdateHotelImagesRequest(
  Guid HotelId,
  string ImageUrl
  );

public record UpdateHotelImagesResponse(
  Guid HotelId,
  bool IsPrimary
  );
