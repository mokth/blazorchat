using System.ComponentModel.DataAnnotations;

namespace blazorchat.Models;

public class Group
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public string CreatedBy { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? AvatarUrl { get; set; }
    
    // Navigation property for members
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
}

public class GroupMember
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string GroupId { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsAdmin { get; set; } = false;
    
    // Navigation properties
    public Group? Group { get; set; }
    public User? User { get; set; }
}
