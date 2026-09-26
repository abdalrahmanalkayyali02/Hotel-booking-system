namespace HBS.API.Dtos.Authentication.LogIn;

public record LogInRequest(
    string Email,
    string Password
);

public record LogInResponse(
    Guid UserId,
    string AccessToken,
    string RefreshToken
);