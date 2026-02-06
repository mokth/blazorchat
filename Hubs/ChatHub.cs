using Microsoft.AspNetCore.SignalR;
using blazorchat.Models;
using blazorchat.Services;

namespace blazorchat.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly FileUploadService _fileUploadService;

    public ChatHub(IChatService chatService, FileUploadService fileUploadService)
    {
        _chatService = chatService;
        _fileUploadService = fileUploadService;
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
            await SendUpdatedUserLists();
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
        var messages = await _chatService.GetMessagesForUserAsync(userId);
        await Clients.Caller.SendAsync("LoadMessages", messages);
        
        // Notify all users about the new connection
        await Clients.All.SendAsync("UserConnected", userName);
        
        // Send updated user list to all clients
        await SendUpdatedUserLists();
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

        await _chatService.AddMessageAsync(chatMessage);
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
        var fileUrl = await _fileUploadService.SaveFileAsync(imageData, fileName, MessageType.Image);
        var chatMessage = new ChatMessage
        {
            User = senderName,
            SenderId = senderId,
            RecipientId = recipientId,
            IsGroup = isGroup,
            Content = fileUrl,
            FileUrl = fileUrl,
            Type = MessageType.Image,
            FileName = fileName,
            Timestamp = DateTime.UtcNow
        };

        await _chatService.AddMessageAsync(chatMessage);
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
        var voiceFileName = $"voice_{Guid.NewGuid()}.webm";
        var fileUrl = await _fileUploadService.SaveFileAsync(audioData, voiceFileName, MessageType.Voice);
        var chatMessage = new ChatMessage
        {
            User = senderName,
            SenderId = senderId,
            RecipientId = recipientId,
            IsGroup = isGroup,
            Content = fileUrl,
            FileUrl = fileUrl,
            Type = MessageType.Voice,
            Duration = duration,
            Timestamp = DateTime.UtcNow
        };

        await _chatService.AddMessageAsync(chatMessage);
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
        var fileUrl = await _fileUploadService.SaveFileAsync(fileData, fileName, MessageType.File);
        var chatMessage = new ChatMessage
        {
            User = senderName,
            SenderId = senderId,
            RecipientId = recipientId,
            IsGroup = isGroup,
            Content = fileUrl,
            FileUrl = fileUrl,
            Type = MessageType.File,
            FileName = fileName,
            Timestamp = DateTime.UtcNow
        };

        await _chatService.AddMessageAsync(chatMessage);
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
        await _chatService.MarkMessageAsReadAsync(messageId);
        await Clients.All.SendAsync("MessageRead", messageId);
    }

    private async Task SendUpdatedUserLists()
    {
        var users = _chatService.GetOnlineUsers();
        foreach (var user in users)
        {
            var otherUsers = users.Where(onlineUser => onlineUser.Id != user.Id).ToList();
            await Clients.Client(user.ConnectionId).SendAsync("UpdateUserList", otherUsers);
        }
    }
}
