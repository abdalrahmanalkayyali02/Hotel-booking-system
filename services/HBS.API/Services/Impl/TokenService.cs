using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HBS.API.Services.Interface;
using HBS.API.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace HBS.API.Services.Impl;

public class TokenService : ITokenService
{
    private readonly TokenSettings _tokenSettings;
    private readonly IDatabase _redisDatabase;

    public TokenService(IOptions<TokenSettings> tokenSettings,
        IConnectionMultiplexer connectionMultiplexer)
    {
        _tokenSettings = tokenSettings.Value;
        _redisDatabase = connectionMultiplexer.GetDatabase();
    }

    public string GenerateAccessToken(Guid userId, string email, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.SecretKey));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _tokenSettings.Issuer,
            audience: _tokenSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_tokenSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task StoreRefreshTokenAsync(string refreshToken, Guid userId, TimeSpan expiry)
    {
        var key = $"refreshToken:{refreshToken}";

        await _redisDatabase.StringSetAsync(
            key,
            userId.ToString(),
            expiry
        );
    }

    public async Task<Guid?> GetUserIdByRefreshTokenAsync(string refreshToken)
    {
        var key = $"refreshToken:{refreshToken}";

        var value = await _redisDatabase.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        if (!Guid.TryParse(value.ToString(), out var userId))
        {
            return null;
        }

        return userId;
    }

    public async Task DeleteRefreshTokenAsync(string refreshToken)
    {
        var key = $"refreshToken:{refreshToken}";

        await _redisDatabase.KeyDeleteAsync(key);
    }

    public async Task BlacklistTokenAsync(string jti, TimeSpan expiry)
    {
        var key = $"blacklist:{jti}";

        await _redisDatabase.StringSetAsync(
            key,
            "1",
            expiry
        );
    }

    public async Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        var key = $"blacklist:{jti}";

        return await _redisDatabase.KeyExistsAsync(key);
    }
}