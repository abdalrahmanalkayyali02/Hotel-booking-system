namespace HBS.API.Dtos.Hotels.UploadHotelImages;

public record UploadHotelImagesRequest(
  Guid HotelId,
  string ImageUrl);
