namespace blazorchat.Models;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public int? Duration { get; set; } // For voice messages in seconds
    public bool IsRead { get; set; }
    public bool IsSent { get; set; } = true;
}
