using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace blazorchat.Models;

public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public bool EmailVerified { get; set; } = false;
    
    public string? EmailVerificationToken { get; set; }
    
    public string? ResetPasswordToken { get; set; }
    
    public DateTime? ResetPasswordTokenExpiry { get; set; }
    
    [NotMapped] // ConnectionId is for active sessions only, not persisted
    public string ConnectionId { get; set; } = string.Empty;
    
    [NotMapped] // IsOnline is derived from active connections, not persisted
    public bool IsOnline { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    
    public string? AvatarUrl { get; set; }
}
