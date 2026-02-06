using Microsoft.AspNetCore.Components;
using blazorchat.Models;

namespace blazorchat.Components.Shared;

public partial class MessageBubble : ComponentBase
{
    [Parameter] public ChatMessage Message { get; set; } = new();
    [Parameter] public bool IsCurrentUser { get; set; }
    [Parameter] public bool IsGrouped { get; set; }

    private string FormatDuration(int seconds)
    {
        var minutes = seconds / 60;
        var secs = seconds % 60;
        return $"{minutes}:{secs:D2}";
    }
}
