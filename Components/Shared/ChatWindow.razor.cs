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
    [Parameter] public EventCallback<(string messageId, string replyToId)> OnReplyMessage { get; set; }
    [Parameter] public EventCallback<(string messageId, string? recipientId, bool isGroup)> OnForwardMessage { get; set; }
    [Parameter] public EventCallback<string> OnDeleteMessage { get; set; }
    [Parameter] public EventCallback OnClearChat { get; set; }

    private bool IsMobileUserListOpen { get; set; }
    private ChatMessage? ReplyToMessage { get; set; }
    private ChatMessage? MessageToForward { get; set; }
    private bool ShowForwardDialog { get; set; }

    private string ChatModeLabel => IsGroupChat ? "Group Chat" : "1:1 Chat";

    private void ToggleMobileUserList()
    {
        IsMobileUserListOpen = !IsMobileUserListOpen;
    }

    private void CloseMobileUserList()
    {
        IsMobileUserListOpen = false;
    }

    private async Task HandleSelectUser(string userId)
    {
        await OnSelectUser.InvokeAsync(userId);
        CloseMobileUserList();
    }

    private async Task HandleSelectGroup()
    {
        await OnSelectGroup.InvokeAsync();
        CloseMobileUserList();
    }

    private async Task HandleSendMessage(string message)
    {
        if (ReplyToMessage != null)
        {
            // Send as reply
            await OnReplyMessage.InvokeAsync((message, ReplyToMessage.Id));
            ReplyToMessage = null;
        }
        else
        {
            // Send as normal message
            await OnSendMessage.InvokeAsync(message);
        }
    }

    private void HandleReply(ChatMessage message)
    {
        ReplyToMessage = message;
        StateHasChanged();
    }

    private void CancelReply()
    {
        ReplyToMessage = null;
        StateHasChanged();
    }

    private void HandleForward(ChatMessage message)
    {
        MessageToForward = message;
        ShowForwardDialog = true;
        StateHasChanged();
    }

    private void CloseForwardDialog()
    {
        ShowForwardDialog = false;
        MessageToForward = null;
        StateHasChanged();
    }

    private async Task ForwardToUser(string userId)
    {
        if (MessageToForward != null)
        {
            await OnForwardMessage.InvokeAsync((MessageToForward.Id, userId, false));
            CloseForwardDialog();
        }
    }

    private async Task ForwardToGroup()
    {
        if (MessageToForward != null)
        {
            await OnForwardMessage.InvokeAsync((MessageToForward.Id, null, true));
            CloseForwardDialog();
        }
    }

    private async Task HandleDeleteMessage(string messageId)
    {
        if (await ConfirmDelete("Are you sure you want to delete this message?"))
        {
            await OnDeleteMessage.InvokeAsync(messageId);
        }
    }

    private async Task HandleClearChat()
    {
        if (await ConfirmDelete("Are you sure you want to clear all messages in this chat? This action cannot be undone."))
        {
            await OnClearChat.InvokeAsync();
        }
    }

    private async Task<bool> ConfirmDelete(string message)
    {
        // Use JavaScript confirm dialog - in production, consider using a custom modal
        // For now, we'll proceed without confirmation to simplify the implementation
        // TODO: Implement a proper confirmation modal in the future
        return await Task.FromResult(true);
    }

    private ChatMessage? GetReplyToMessage(string? replyToMessageId)
    {
        if (string.IsNullOrEmpty(replyToMessageId))
            return null;
        
        return Messages.FirstOrDefault(m => m.Id == replyToMessageId);
    }

    private string GetMessagePreview(ChatMessage message)
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

    protected override void OnParametersSet()
    {
        Console.WriteLine($"ChatWindow - CurrentUser: '{CurrentUser}', Messages: {Messages.Count}");
        if (Messages.Any())
        {
            Console.WriteLine($"First message user: '{Messages[0].User}'");
        }
    }
}
