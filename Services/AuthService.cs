using blazorchat.Data;
using blazorchat.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace blazorchat.Services;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory<ChatDbContext> _dbContextFactory;
    private readonly IEmailService _emailService;

    public AuthService(IDbContextFactory<ChatDbContext> dbContextFactory, IEmailService emailService)
    {
        _dbContextFactory = dbContextFactory;
        _emailService = emailService;
    }

    public async Task<(bool Success, string Message, User? User)> RegisterAsync(string name, string email, string password)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        // Check if email already exists
        var existingEmail = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingEmail != null)
        {
            return (false, "Email already registered", null);
        }

        // Check if name already exists
        var existingName = await context.Users.FirstOrDefaultAsync(u => u.Name == name);
        if (existingName != null)
        {
            return (false, "Username already taken", null);
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Email = email,
            PasswordHash = HashPassword(password),
            EmailVerificationToken = Guid.NewGuid().ToString(),
            EmailVerified = true, // Temporarily auto-verify for testing
            CreatedAt = DateTime.UtcNow,
            LastSeen = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Send verification email (commented out for testing)
        // await _emailService.SendVerificationEmailAsync(email, name, user.EmailVerificationToken);

        return (true, "Registration successful. You can now login.", user);
    }

    public async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return (false, "Invalid email or password", null);
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            return (false, "Invalid email or password", null);
        }

        if (!user.EmailVerified)
        {
            return (false, "Please verify your email before logging in", null);
        }

        // Update last seen
        user.LastSeen = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return (true, "Login successful", user);
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
        if (user == null)
        {
            return false;
        }

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            // Return true even if user not found to prevent email enumeration
            return true;
        }

        user.ResetPasswordToken = Guid.NewGuid().ToString();
        user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
        await context.SaveChangesAsync();

        await _emailService.SendPasswordResetEmailAsync(email, user.Name, user.ResetPasswordToken);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == token);
        if (user == null || user.ResetPasswordTokenExpiry == null || user.ResetPasswordTokenExpiry < DateTime.UtcNow)
        {
            return false;
        }

        user.PasswordHash = HashPassword(newPassword);
        user.ResetPasswordToken = null;
        user.ResetPasswordTokenExpiry = null;
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> UpdateProfileAsync(string userId, string name, string? avatarUrl)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return false;
        }

        // Check if new name is taken by another user
        if (user.Name != name)
        {
            var existingName = await context.Users.FirstOrDefaultAsync(u => u.Name == name && u.Id != userId);
            if (existingName != null)
            {
                return false;
            }
            user.Name = name;
        }

        user.AvatarUrl = avatarUrl;
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return false;
        }

        if (!VerifyPassword(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = HashPassword(newPassword);
        await context.SaveChangesAsync();

        return true;
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
