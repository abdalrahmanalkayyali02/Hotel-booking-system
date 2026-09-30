namespace HBS.API.Dtos.Hotels.UploadHotelImages;

public record UploadHotelImagesRequest(
  IFormFile Image
  );

public record UploadHotelImageResponse(
  Guid Id,
  Guid HotelId,
  string ImageUrl,
  bool IsPrimary
  );
