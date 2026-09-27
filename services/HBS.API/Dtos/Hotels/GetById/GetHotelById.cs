namespace HBS.API.Dtos.Hotels.GetById;

public record GetHotelByIdResponse(
  Guid HotelId,
  string Name,
  string? Description,
  string Address,
  string Country,
  string City,
  float StarRating,
  string PhoneNumberCountryCode,
  string PhoneNumber,
  string? Email,
  Guid ManagerId
  );
