namespace blazorchat.Models;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string User { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? FileName { get; set; }
    public int? Duration { get; set; } // For voice messages in seconds
    public bool IsRead { get; set; } = false;
    public string? FileUrl { get; set; }
}
