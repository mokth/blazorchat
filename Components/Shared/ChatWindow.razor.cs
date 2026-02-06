using Microsoft.AspNetCore.Components;
using blazorchat.Models;

namespace blazorchat.Components.Shared;

public partial class ChatWindow : ComponentBase
{
    [Parameter] public List<ChatMessage> Messages { get; set; } = new();
    [Parameter] public string CurrentUser { get; set; } = "";
    [Parameter] public string CurrentUserId { get; set; } = "";
    [Parameter] public List<User> OnlineUsers { get; set; } = new();
    [Parameter] public string TypingUser { get; set; } = "";
    [Parameter] public string? SelectedUserId { get; set; }
    [Parameter] public string SelectedChatName { get; set; } = "Select a contact";
    [Parameter] public bool IsGroupChat { get; set; }
    [Parameter] public bool CanSendMessages { get; set; }
    [Parameter] public Dictionary<string, int> UnreadCounts { get; set; } = new();
    [Parameter] public EventCallback<string> OnSelectUser { get; set; }
    [Parameter] public EventCallback OnSelectGroup { get; set; }
    [Parameter] public EventCallback<string> OnSendMessage { get; set; }
    [Parameter] public EventCallback<(string imageData, string fileName)> OnSendImage { get; set; }
    [Parameter] public EventCallback<(string audioData, int duration)> OnSendVoice { get; set; }
    [Parameter] public EventCallback<(string fileData, string fileName)> OnSendFile { get; set; }
    [Parameter] public EventCallback OnTyping { get; set; }

    private string ChatModeLabel => IsGroupChat ? "Group Chat" : "1:1 Chat";

    protected override void OnParametersSet()
    {
        Console.WriteLine($"ChatWindow - CurrentUser: '{CurrentUser}', Messages: {Messages.Count}");
        if (Messages.Any())
        {
            Console.WriteLine($"First message user: '{Messages[0].User}'");
        }
    }
}
