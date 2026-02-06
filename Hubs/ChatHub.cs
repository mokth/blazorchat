using Microsoft.AspNetCore.SignalR;
using blazorchat.Models;
using blazorchat.Services;

namespace blazorchat.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = _chatService.GetUserByConnectionId(Context.ConnectionId);
        if (user != null)
        {
            _chatService.RemoveUser(user.Id);
            await Clients.All.SendAsync("UserDisconnected", user.Name);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task UserConnected(string userId, string userName)
    {
        var user = new User
        {
            Id = userId,
            Name = userName,
            ConnectionId = Context.ConnectionId,
            IsOnline = true
        };

        _chatService.AddUser(user);
        
        // Send existing messages to the new user
        var messages = _chatService.GetMessagesForUser(userId);
        await Clients.Caller.SendAsync("LoadMessages", messages);
        
        // Notify all users about the new connection
        await Clients.All.SendAsync("UserConnected", userName);
        
        // Send updated user list to all clients
        var users = _chatService.GetOnlineUsers();
        await Clients.All.SendAsync("UpdateUserList", users);
    }

    public async Task SendMessage(string senderId, string senderName, string message, string? recipientId, bool isGroup)
    {
        var chatMessage = new ChatMessage
        {
            User = senderName,
            SenderId = senderId,
            RecipientId = recipientId,
            IsGroup = isGroup,
            Content = message,
            Type = MessageType.Text,
            Timestamp = DateTime.UtcNow
        };

        _chatService.AddMessage(chatMessage);
        if (isGroup)
        {
            await Clients.All.SendAsync("ReceiveMessage", chatMessage);
            return;
        }

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return;
        }

        var recipient = _chatService.GetUserById(recipientId);
        if (recipient != null)
        {
            await Clients.Clients(Context.ConnectionId, recipient.ConnectionId)
                .SendAsync("ReceiveMessage", chatMessage);
        }
    }

    public async Task SendImage(string senderId, string senderName, string imageData, string fileName, string? recipientId, bool isGroup)
    {
        var chatMessage = new ChatMessage
        {
            User = senderName,
            SenderId = senderId,
            RecipientId = recipientId,
            IsGroup = isGroup,
            Content = imageData,
            Type = MessageType.Image,
            FileName = fileName,
            Timestamp = DateTime.UtcNow
        };

        _chatService.AddMessage(chatMessage);
        if (isGroup)
        {
            await Clients.All.SendAsync("ReceiveMessage", chatMessage);
            return;
        }

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return;
        }

        var recipient = _chatService.GetUserById(recipientId);
        if (recipient != null)
        {
            await Clients.Clients(Context.ConnectionId, recipient.ConnectionId)
                .SendAsync("ReceiveMessage", chatMessage);
        }
    }

    public async Task SendVoiceMessage(string senderId, string senderName, string audioData, int duration, string? recipientId, bool isGroup)
    {
        var chatMessage = new ChatMessage
        {
            User = senderName,
            SenderId = senderId,
            RecipientId = recipientId,
            IsGroup = isGroup,
            Content = audioData,
            Type = MessageType.Voice,
            Duration = duration,
            Timestamp = DateTime.UtcNow
        };

        _chatService.AddMessage(chatMessage);
        if (isGroup)
        {
            await Clients.All.SendAsync("ReceiveMessage", chatMessage);
            return;
        }

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return;
        }

        var recipient = _chatService.GetUserById(recipientId);
        if (recipient != null)
        {
            await Clients.Clients(Context.ConnectionId, recipient.ConnectionId)
                .SendAsync("ReceiveMessage", chatMessage);
        }
    }

    public async Task SendFile(string senderId, string senderName, string fileData, string fileName, string? recipientId, bool isGroup)
    {
        var chatMessage = new ChatMessage
        {
            User = senderName,
            SenderId = senderId,
            RecipientId = recipientId,
            IsGroup = isGroup,
            Content = fileData,
            Type = MessageType.File,
            FileName = fileName,
            Timestamp = DateTime.UtcNow
        };

        _chatService.AddMessage(chatMessage);
        if (isGroup)
        {
            await Clients.All.SendAsync("ReceiveMessage", chatMessage);
            return;
        }

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return;
        }

        var recipient = _chatService.GetUserById(recipientId);
        if (recipient != null)
        {
            await Clients.Clients(Context.ConnectionId, recipient.ConnectionId)
                .SendAsync("ReceiveMessage", chatMessage);
        }
    }

    public async Task UserTyping(string senderId, string senderName, string? recipientId, bool isGroup)
    {
        if (isGroup)
        {
            await Clients.Others.SendAsync("UserTyping", senderId, senderName, recipientId, isGroup);
            return;
        }

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return;
        }

        var recipient = _chatService.GetUserById(recipientId);
        if (recipient != null)
        {
            await Clients.Client(recipient.ConnectionId)
                .SendAsync("UserTyping", senderId, senderName, recipientId, isGroup);
        }
    }

    public async Task MessageRead(string messageId)
    {
        _chatService.MarkMessageAsRead(messageId);
        await Clients.All.SendAsync("MessageRead", messageId);
    }
}
