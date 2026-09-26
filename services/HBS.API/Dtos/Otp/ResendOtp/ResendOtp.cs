namespace HBS.API.Dtos.Otp.ResendOtp;

public record ResendOtpRequest(
    string Email
);

public record ResendOtpResponse(
    string Email,
    DateTime ExpiresAt
);