namespace HBS.API.integrations.Interface;

public interface IEmailService
{
    Task SendOtpEmailAsync(string emial, string otpCode);
}