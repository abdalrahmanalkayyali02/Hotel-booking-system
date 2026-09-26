using HBS.API.integrations.Interface;
using HBS.API.Settings;
using MailKit;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace HBS.API.integrations.Provider;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendOtpEmailAsync(string email, string otpCode)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(email));

        message.Subject = "Email Verification Otp";

        message.Body = new TextPart("plain")
        {
            Text = $"Your verification code is: {otpCode}"
        };

        //using var smtp = new SmtpClient();

        //tempporary code for testing email issues 
        using var smtp = new SmtpClient(
            new ProtocolLogger(Console.OpenStandardOutput())
        );

        Console.WriteLine($"SMTP Host: '{_emailSettings.SmtpHost}'");
        Console.WriteLine($"SMTP Port: {_emailSettings.SmtpPort}");
        Console.WriteLine($"SMTP Sender: '{_emailSettings.SenderEmail}'");

        smtp.CheckCertificateRevocation = false;
        smtp.Timeout = 20000;

        await smtp.ConnectAsync(
            _emailSettings.SmtpHost,
            _emailSettings.SmtpPort,
            SecureSocketOptions.SslOnConnect
        );

        await smtp.AuthenticateAsync(
            _emailSettings.SenderEmail,
            _emailSettings.Password
        );

        Console.WriteLine($"Sending OTP email to {email}");
        await smtp.SendAsync(message);
        Console.WriteLine("OTP email sent successfully");

        await smtp.DisconnectAsync(true);

    }
}