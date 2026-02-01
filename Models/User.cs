namespace blazorchat.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public bool IsOnline { get; set; } = false;
    public DateTime LastSeen { get; set; } = DateTime.Now;
    public string? AvatarUrl { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
}
