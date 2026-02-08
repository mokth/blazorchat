namespace blazorchat.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string name, string token);
    Task SendPasswordResetEmailAsync(string email, string name, string token);
}
