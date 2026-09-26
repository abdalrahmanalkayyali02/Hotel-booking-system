using HBS.API.Shared.enums;

namespace HBS.API.Dtos.Users.Delete;

public record SoftDeleteUserByIdResponse(
    Guid UserId,
    UserStatus Status
);