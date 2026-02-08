using Microsoft.Extensions.Configuration;

namespace blazorchat.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(string email, string name, string token)
    {
        var baseUrl = _configuration["Email:BaseUrl"] ?? "http://localhost:5000";
        var verificationLink = $"{baseUrl}/verify-email?token={token}";

        // For development, just log the verification link
        _logger.LogInformation("Email Verification Link for {Email}: {Link}", email, verificationLink);

        // TODO: In production, implement actual email sending using SMTP or email service provider
        // Example: SendGrid, MailKit, etc.
        
        await Task.CompletedTask;
    }

    public async Task SendPasswordResetEmailAsync(string email, string name, string token)
    {
        var baseUrl = _configuration["Email:BaseUrl"] ?? "http://localhost:5000";
        var resetLink = $"{baseUrl}/reset-password?token={token}";

        // For development, just log the reset link
        _logger.LogInformation("Password Reset Link for {Email}: {Link}", email, resetLink);

        // TODO: In production, implement actual email sending using SMTP or email service provider
        
        await Task.CompletedTask;
    }
}
