using HBS.API.Dtos.Authentication.ChangePassword;
using HBS.API.Dtos.Authentication.ForgotPassword;
using HBS.API.Dtos.Authentication.LogIn;
using HBS.API.Shared.Result;
using HBS.API.Dtos.Authentication.RefreshToken;
using HBS.API.Dtos.Authentication.LogOut;

namespace HBS.API.Services.Interface;

public interface IAuthService
{
    Task<Result<LogInResponse>> LogIn(LogInRequest request);
    Task<Result<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request);
    Task<Result> LogOut(Guid userId, string jti, DateTime accessTokenExpiresAt, LogOutRequest request);
    Task<Result> ForgotPassword(ForgotPasswordRequest request);
    Task<Result<VerifyForgotPasswordOtpResponse>> VerifyForgotPasswordOtp(VerifyForgotPasswordOtpRequest request);
    Task<Result> ResetPassword(ResetPasswordRequest request);
    Task<Result> ChangePassword(Guid userId, ChangePasswordRequest request);
}