namespace HBS.API.Dtos.Authentication.ForgotPassword;

public record ForgotPasswordRequest(
    string Email
);

public record VerifyForgotPasswordOtpRequest(
    string Email,
    string OtpCode
);

public record VerifyForgotPasswordOtpResponse(
    string ResetToken);

public record ResetPasswordRequest(
    string ResetToken,
    string NewPassword
);