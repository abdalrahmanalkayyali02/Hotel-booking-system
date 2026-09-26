namespace HBS.API.Dtos.Authentication.RefreshToken;

public record RefreshTokenRequest(
    string RefreshToken
);

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken);