using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using HBS.API.Db.models;
using HBS.API.Db.Repo.Interface;
using HBS.API.Dtos.Authentication.ChangePassword;
using HBS.API.Dtos.Authentication.ForgotPassword;
using HBS.API.Dtos.Authentication.LogIn;
using HBS.API.Dtos.Authentication.LogOut;
using HBS.API.Dtos.Authentication.RefreshToken;
using HBS.API.integrations.Interface;
using HBS.API.Services.Interface;
using HBS.API.Settings;
using HBS.API.Shared.enums;
using HBS.API.Shared.Result;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace HBS.API.Services.Impl;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly TokenSettings _tokenSettings;
    private readonly IOtpRepository _otpRepository;
    private readonly IEmailService _emailService;
    private readonly IDatabase _redisDatabase;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

    public AuthService(IUserRepository userRepository,
        ITokenService tokenService,
        IOptions<TokenSettings> tokenSettings,
        IEmailService emailService,
        IOtpRepository otpRepository,
        IConnectionMultiplexer connectionMultiplexer,
        IValidator<ResetPasswordRequest> resetPasswordValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _tokenSettings = tokenSettings.Value;
        _emailService = emailService;
        _otpRepository = otpRepository;
        _redisDatabase = connectionMultiplexer.GetDatabase();
        _resetPasswordValidator = resetPasswordValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    public async Task<Result<LogInResponse>> LogIn(LogInRequest request)
    {
        var normalizedEmail = request.Email.ToUpperInvariant();
        var user = _userRepository.GetByEmail(normalizedEmail);

        if (user is null)
        {
            return Result<LogInResponse>.Failure(
                Error.Unauthorized(
                    "Auth.InvalidCredentials",
                    "Invalid email or password"
                )
            );
        }

        var passwordIsValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordIsValid)
        {
            return Result<LogInResponse>.Failure(
                Error.Unauthorized(
                    "Auth.InvalidCredentials",
                    "Invalid email or password"
                )
            );
        }

        if (!user.IsEmailConfirmed)
        {
            return Result<LogInResponse>.Failure(
                Error.Unauthorized(
                    "Auth.EmailNotVerified",
                    "Email address has not been verified"
                )
            );
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<LogInResponse>.Failure(
                Error.Unauthorized(
                    "Auth.UserNotActive",
                    "This user account is not active"
                )
            );
        }

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.Email,
            user.RoleId.ToString()
        );

        var refreshToken = _tokenService.GenerateRefreshToken();

        await _tokenService.StoreRefreshTokenAsync(
            refreshToken,
            user.Id,
            TimeSpan.FromDays(
                _tokenSettings.RefreshTokenExpirationDays
            )
        );

        var response = new LogInResponse(
            user.Id,
            accessToken,
            refreshToken
        );

        return Result<LogInResponse>.Success(response);
    }

    public async Task<Result<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest request)
    {
        var userId = await _tokenService.GetUserIdByRefreshTokenAsync(request.RefreshToken);

        if (userId is null)
        {
            return Result<RefreshTokenResponse>.Failure(
                Error.Unauthorized(
                    "Auth.InvalidRefreshToken",
                    "Invalid or expired refresh token"));
        }

        var user = _userRepository.GetById(userId.Value);

        if (user is null)
        {
            return Result<RefreshTokenResponse>.Failure(
                Error.Unauthorized(
                    "Auth.InvalidRefreshToken",
                    "Invalid or expired refresh token"));
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<RefreshTokenResponse>.Failure(
                Error.Unauthorized(
                    "Auth.UserNotActive",
                    "This user account is not active"
                )
            );
        }

        var newAccessToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.Email,
            user.RoleId.ToString()
        );

        var newRefreshToken =
            _tokenService.GenerateRefreshToken();

        await _tokenService.DeleteRefreshTokenAsync(
            request.RefreshToken
        );

        await _tokenService.StoreRefreshTokenAsync(
            newRefreshToken,
            user.Id,
            TimeSpan.FromDays(
                _tokenSettings.RefreshTokenExpirationDays
            )
        );

        var response = new RefreshTokenResponse(
            newAccessToken,
            newRefreshToken
        );

        return Result<RefreshTokenResponse>.Success(response);
    }

    public async Task<Result> LogOut(Guid userId, string jti, DateTime accessTokenExpiresAt, LogOutRequest request)
    {
        var refreshTokenUserId = await _tokenService.GetUserIdByRefreshTokenAsync(request.RefreshToken);

        if (refreshTokenUserId is null || refreshTokenUserId.Value != userId)
        {
            return Result.Failure(
                Error.Unauthorized(
                    "Auth.InvalidRefreshToken",
                    "Invalid refresh token"
                )
            );
        }

        await _tokenService.DeleteRefreshTokenAsync(request.RefreshToken);

        var remainingLifetime = accessTokenExpiresAt - DateTime.UtcNow;

        if (remainingLifetime > TimeSpan.Zero)
        {
            await _tokenService.BlacklistTokenAsync(jti, remainingLifetime);
        }

        return Result.Success();
    }

    public async Task<Result> ForgotPassword(ForgotPasswordRequest request)
    {
        var normalizedEmail = request.Email.ToUpperInvariant();

        var user = _userRepository.GetByEmail(normalizedEmail);

        if (user is null)
        {
            return Result.Failure(
                Error.NotFound(
                    "User.NotFound",
                    "The selected user was not found"
                )
            );
        }

        var latestOtp = _otpRepository.GetLatestForgotPasswordOtp(user.Id);

        if (latestOtp is not null && DateTime.UtcNow < latestOtp.GeneratedAt.AddMinutes(1))
        {
            return Result.Failure(
                Error.Conflict(
                    "Otp.ResendTooSoon",
                    "Please wait before requesting another OTP"
                )
            );
        }

        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var otpBytes = Encoding.UTF8.GetBytes(otpCode);
        var hashedOtpBytes = MD5.HashData(otpBytes);
        var hashedOtp = Convert.ToHexString(hashedOtpBytes);

        var generatedAt = DateTime.UtcNow;

        var newOtp = new Otp
        {
            Id = Guid.CreateVersion7(),
            HashedOtp = hashedOtp,
            Type = OtpType.ForgotPassword,
            Target = OtpTarget.Email,
            UserId = user.Id,
            GeneratedAt = generatedAt,
            ExpiresAt = generatedAt.AddMinutes(5),
            IsUsed = false,
            NumberOfAttempts = 0
        };

        _otpRepository.Add(newOtp);

        await _emailService.SendOtpEmailAsync(
            user.Email,
            otpCode
        );

        return Result.Success();
    }

    public async Task<Result<VerifyForgotPasswordOtpResponse>> VerifyForgotPasswordOtp(VerifyForgotPasswordOtpRequest request)
    {
        var normalizedEmail = request.Email.ToUpperInvariant();

        var user = _userRepository.GetByEmail(normalizedEmail);

        if (user is null)
        {
            return Result<VerifyForgotPasswordOtpResponse>.Failure(
                Error.Unauthorized(
                    "Auth.InvalidOtp",
                    "Invalid or expired OTP"
                )
            );
        }

        var otp = _otpRepository.GetLatestForgotPasswordOtp(user.Id);

        if (otp is null)
        {
            return Result<VerifyForgotPasswordOtpResponse>.Failure(
                Error.Unauthorized(
                    "Auth.InvalidOtp",
                    "Invalid or expired OTP"
                )
            );
        }

        if (otp.IsUsed)
        {
            return Result<VerifyForgotPasswordOtpResponse>.Failure(
                Error.Conflict(
                    "Otp.AlreadyUsed",
                    "This OTP has already been used"
                )
            );
        }

        if (DateTime.UtcNow > otp.ExpiresAt)
        {
            return Result<VerifyForgotPasswordOtpResponse>.Failure(
                Error.Validation(
                    "Otp.Expired",
                    "The OTP has expired"
                )
            );
        }

        const int maxAttempts = 5;

        if (otp.NumberOfAttempts >= maxAttempts)
        {
            return Result<VerifyForgotPasswordOtpResponse>.Failure(
                Error.Conflict(
                    "Otp.MaxAttemptsReached",
                    "The maximum number of attempts for this OTP has been reached"
                )
            );
        }

        var otpBytes = Encoding.UTF8.GetBytes(request.OtpCode);
        var hashedOtpBytes = MD5.HashData(otpBytes);
        var submittedHashedOtp = Convert.ToHexString(hashedOtpBytes);

        if (submittedHashedOtp != otp.HashedOtp)
        {
            otp.NumberOfAttempts++;

            _otpRepository.Update(otp);

            return Result<VerifyForgotPasswordOtpResponse>.Failure(
                Error.Validation(
                    "Otp.Wrong",
                    "The OTP entered is incorrect"
                )
            );
        }

        var resetTokenBytes = RandomNumberGenerator.GetBytes(64);

        var resetToken = Convert.ToBase64String(resetTokenBytes);

        var resetTokenKey = $"passwordReset:{resetToken}";

        await _redisDatabase.StringSetAsync(
            resetTokenKey,
            user.Id.ToString(),
            TimeSpan.FromMinutes(10)
        );

        otp.IsUsed = true;

        _otpRepository.Update(otp);

        var response =
            new VerifyForgotPasswordOtpResponse(
                resetToken
            );

        return Result<VerifyForgotPasswordOtpResponse>.Success(response);

    }

    public async Task<Result> ResetPassword(ResetPasswordRequest request)
    {
        var validationResult = _resetPasswordValidator.Validate(request);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => (object)group
                        .Select(error => error.ErrorMessage)
                        .ToArray()
                );
            return Result.Failure(Error.Validation("User.Validation",
                "One or more validation errors occurred", errors));
        }
        
        var resetTokenKey = $"passwordReset:{request.ResetToken}";

        var value =
            await _redisDatabase.StringGetAsync(
                resetTokenKey
            );

        if (value.IsNullOrEmpty || !Guid.TryParse(value.ToString(), out var userId))
        {
            return Result.Failure(
                Error.Unauthorized(
                    "Auth.InvalidResetToken",
                    "Invalid or expired password reset token"
                )
            );
        }

        var user = _userRepository.GetById(userId);

        if (user is null)
        {
            return Result.Failure(
                Error.Unauthorized(
                    "Auth.InvalidResetToken",
                    "Invalid or expired password reset token"
                )
            );
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        user.PasswordHash = newPasswordHash;

        _userRepository.Update(user);

        await _redisDatabase.KeyDeleteAsync(
            resetTokenKey
        );

        return Result.Success();
    }

    public async Task<Result> ChangePassword(Guid userId, ChangePasswordRequest request)
    {
        var validationResult = _changePasswordValidator.Validate(request);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => (object)group
                        .Select(error => error.ErrorMessage)
                        .ToArray()
                );
            return Result.Failure(Error.Validation("User.Validation",
                "One or more validation errors occurred", errors));
        }
        
        var user = _userRepository.GetById(userId);

        if (user is null)
        {
            return Result.Failure(
                Error.Unauthorized("Auth.InvalidUser",
                    "The authenticated user could not be found"));
        }

        var currentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);

        if (!currentPasswordValid)
        {
            return Result.Failure(
                Error.Unauthorized("Auth.InvalidPassword",
            "The current password is incorrect"));
        }

        var newPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        user.PasswordHash = newPassword;
        _userRepository.Update(user);

        return Result.Success();
    }
}