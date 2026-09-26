using HBS.API.Dtos.Authentication.LogIn;
using HBS.API.Dtos.Authentication.RefreshToken;
using HBS.API.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using HBS.API.Shared.Api;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HBS.API.Dtos.Authentication.ForgotPassword;
using HBS.API.Dtos.Authentication.LogOut;
using Microsoft.AspNetCore.Authorization;
using HBS.API.Dtos.Authentication.ChangePassword;

namespace HBS.API.Controller;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult> LogIn([FromBody] LogInRequest request)
    {
        var result = await _authService.LogIn(request);
        return HandleResult(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshToken(request);

        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> LogOut([FromBody] LogOutRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

        var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(expClaim))
        {
            return Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim);

        var expirationUnix =
            long.Parse(expClaim);

        var accessTokenExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expirationUnix).UtcDateTime;

        var result = await _authService.LogOut(userId, jti, accessTokenExpiresAt, request);

        return HandleResult(result);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPassword(request);

        return HandleResult(result);
    }

    [HttpPost("verify-forgot-password-otp")]
    public async Task<ActionResult> VerifyForgotPasswordOtp([FromBody] VerifyForgotPasswordOtpRequest request)
    {
        var result = await _authService.VerifyForgotPasswordOtp(request);

        return HandleResult(result);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPassword(request);

        return HandleResult(result);
    }

    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _authService.ChangePassword(userId, request);

        return HandleResult(result);
    }
}