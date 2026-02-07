using Microsoft.AspNetCore.Components;
using blazorchat.Models;

namespace blazorchat.Components.Shared;

public partial class MessageBubble : ComponentBase
{
    [Parameter] public ChatMessage Message { get; set; } = new();
    [Parameter] public bool IsCurrentUser { get; set; }
    [Parameter] public bool IsGrouped { get; set; }
    [Parameter] public ChatMessage? ReplyToMessage { get; set; }
    [Parameter] public EventCallback<ChatMessage> OnReply { get; set; }
    [Parameter] public EventCallback<ChatMessage> OnForward { get; set; }
    [Parameter] public EventCallback<string> OnDelete { get; set; }

    private string FormatDuration(int seconds)
    {
        var minutes = seconds / 60;
        var secs = seconds % 60;
        return $"{minutes}:{secs:D2}";
    }

    private string GetReplyPreviewText(ChatMessage message)
    {
        return message.Type switch
        {
            MessageType.Text => message.Content.Length > 50 ? message.Content.Substring(0, 50) + "..." : message.Content,
            MessageType.Image => "📷 Image",
            MessageType.Voice => "🎤 Voice message",
            MessageType.File => $"📄 {message.FileName}",
            _ => ""
        };
    }

    private async Task HandleReply()
    {
        await OnReply.InvokeAsync(Message);
    }

    private async Task HandleForward()
    {
        await OnForward.InvokeAsync(Message);
    }

    private async Task HandleDelete()
    {
        await OnDelete.InvokeAsync(Message.Id);
    }
}
