using blazorchat.Models;

namespace blazorchat.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, User? User)> RegisterAsync(string name, string email, string password);
    Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> UpdateProfileAsync(string userId, string name, string? avatarUrl);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    bool VerifyPassword(string password, string passwordHash);
    string HashPassword(string password);
}
