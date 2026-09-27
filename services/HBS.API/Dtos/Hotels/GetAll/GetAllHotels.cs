namespace HBS.API.Dtos.Hotels.GetAll;

public record GetAllHotelsResponse(
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
