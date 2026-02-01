namespace blazorchat.Models;

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public string? AvatarUrl { get; set; }
}
