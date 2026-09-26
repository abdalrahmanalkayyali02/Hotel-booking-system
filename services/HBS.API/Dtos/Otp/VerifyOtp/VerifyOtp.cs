using HBS.API.Shared.enums;

namespace HBS.API.Dtos.Otp.VerifyOtp;

public record VerifyOtpRequest(
    string Email,
    string OtpCode
);

public record VerifyOtpResponse(
    Guid UserId,
    bool IsEmailConfirmed,
    UserStatus Status,
    string Token,
    string RefreshToken
);
