using HBS.API.Shared.enums;

namespace HBS.API.Dtos.Hotels.SoftDelete;

public record SoftDeleteHotelResponse(
  Guid HotelId,
  HotelStatus Status
  );
