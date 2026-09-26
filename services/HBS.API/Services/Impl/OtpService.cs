using System.Security.Cryptography;
using System.Text;
using HBS.API.Db.Repo.Interface;
using HBS.API.Services.Interface;
using HBS.API.Shared.Result;
using HBS.API.Dtos.Otp.VerifyOtp;
using HBS.API.Dtos.Otp.ResendOtp;
using HBS.API.Shared.enums;
using HBS.API.Db.models;
using HBS.API.integrations.Interface;
using HBS.API.Settings;
using Microsoft.Extensions.Options;

namespace HBS.API.Services.Impl;

public class OtpService : IOtpService
{
    private readonly IOtpRepository _otpRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly TokenSettings _tokenSettings;

    public OtpService(IOtpRepository otpRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        ITokenService tokenService,
        IOptions<TokenSettings> tokenSettings)
    {
        _otpRepository = otpRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _tokenService = tokenService;
        _tokenSettings = tokenSettings.Value;
    }

    public async Task<Result<VerifyOtpResponse>> VerifyOtp(VerifyOtpRequest request)
    {
        var normalizedEmail = request.Email.ToUpperInvariant();

        var user = _userRepository.GetByEmail(normalizedEmail);

        if (user is null)
        {
            return Result<VerifyOtpResponse>.Failure(
                Error.NotFound(
                    "User.NotFound",
                    "The selected user was not found"
                )
            );
        }

        var otp = _otpRepository.GetLatestRegistrationOtp(user.Id);
        if (otp is null)
        {
            return Result<VerifyOtpResponse>.Failure(
                Error.NotFound("Otp.NotFound",
                    "No Otp registration was found for this user"));
        }

        if (otp.IsUsed)
        {
            return Result<VerifyOtpResponse>.Failure(
                Error.Conflict("Otp.AlreadyUsed",
                    "This Otp has already been used"));
        }

        if (DateTime.UtcNow > otp.ExpiresAt)
        {
            return Result<VerifyOtpResponse>.Failure(
                Error.Validation("Otp.Expired",
                    "The Otp has expired"));
        }

        const int maxAttempts = 5;
        if (otp.NumberOfAttempts >= maxAttempts)
        {
            return Result<VerifyOtpResponse>.Failure(
                Error.Conflict("Otp.MaxAttemptsReached",
                    "The maximum number of attempts for this Otp has been reached"));
        }

        var otpBytes = Encoding.UTF8.GetBytes(request.OtpCode);
        var hashedOtpBytes = MD5.HashData(otpBytes);
        var submittedhashedOtp = Convert.ToHexString(hashedOtpBytes);

        if (submittedhashedOtp != otp.HashedOtp)
        {
            otp.NumberOfAttempts++;
            _otpRepository.Update(otp);

            return Result<VerifyOtpResponse>.Failure(
                Error.Validation("Otp.Wrong",
                    "The Otp entered is incorrect"));
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

        otp.IsUsed = true;
        _otpRepository.Update(otp);

        user.IsEmailConfirmed = true;
        user.Status = UserStatus.Active;
        user.VerifiedAt = DateTime.UtcNow;
        _userRepository.Update(user);

        var response = new VerifyOtpResponse(
            user.Id,
            user.IsEmailConfirmed,
            user.Status,
            accessToken,
            refreshToken
        );

        return Result<VerifyOtpResponse>.Success(response);
    }//End of VerifyOtp

    public async Task<Result<ResendOtpResponse>> ResendOtp(ResendOtpRequest request)
    {
        var normalizedEmail = request.Email.ToUpperInvariant();

        var user = _userRepository.GetByEmail(normalizedEmail);

        if (user is null)
        {
            return Result<ResendOtpResponse>.Failure(
                Error.NotFound(
                    "User.NotFound",
                    "The selected user was not found"
                )
            );
        }

        if (user.IsEmailConfirmed)
        {
            return Result<ResendOtpResponse>.Failure(
                Error.Conflict("User.AlreadyVerified",
                    "The user's email has already been verified"));
        }

        var latestOtp = _otpRepository.GetLatestRegistrationOtp(user.Id);

        if (latestOtp is not null &&
            DateTime.UtcNow < latestOtp.GeneratedAt.AddMinutes(1))
        {
            return Result<ResendOtpResponse>.Failure(
                Error.Conflict(
                    "Otp.ResendTooSoon",
                    "Please wait before requesting another Otp"
                )
            );
        }

        //Generating a new Otp
        var otpCode = RandomNumberGenerator
            .GetInt32(100000, 1000000)
            .ToString();

        var otpBytes = Encoding.UTF8.GetBytes(otpCode);
        var hashedOtpBytes = MD5.HashData(otpBytes);
        var hashedOtp = Convert.ToHexString(hashedOtpBytes);

        //creating a new Otp record and storing it
        var generatedAt = DateTime.UtcNow;

        Otp newOtp = new Otp
        {
            Id = Guid.CreateVersion7(),
            HashedOtp = hashedOtp,
            Type = OtpType.Registration,
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

        var response = new ResendOtpResponse(
            user.Email,
            newOtp.ExpiresAt
        );

        return Result<ResendOtpResponse>.Success(response);
    }
}