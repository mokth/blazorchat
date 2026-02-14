namespace blazorchat.Models;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string User { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string? RecipientId { get; set; }
    public bool IsGroup { get; set; }
    public string? GroupId { get; set; }  // For private groups
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? FileName { get; set; }
    public int? Duration { get; set; } // For voice messages in seconds
    public bool IsRead { get; set; }
    public bool IsDelivered { get; set; } = true;
    public string? FileUrl { get; set; }
    public string? ReplyToMessageId { get; set; }
    public string? ForwardedFromMessageId { get; set; }
}
