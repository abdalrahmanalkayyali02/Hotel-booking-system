namespace HBS.API.Services.Interface;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, string role);
    string GenerateRefreshToken();
    Task StoreRefreshTokenAsync(string refreshToken, Guid userId, TimeSpan expiry);
    Task<Guid?> GetUserIdByRefreshTokenAsync(string refreshToken);
    Task DeleteRefreshTokenAsync(string refreshToken);
    Task BlacklistTokenAsync(string jti, TimeSpan expiry);
    Task<bool> IsTokenBlacklistedAsync(string jti);
}