namespace HBS.API.Dtos.Authentication.ChangePassword;

public record ChangePasswordRequest(
    string CurrentPassword, 
    string NewPassword
    );