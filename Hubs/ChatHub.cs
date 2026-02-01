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

    public async Task SendMessage(string userId, string userName, string message)
    {
        var chatMessage = new ChatMessage
        {
            UserId = userId,
            UserName = userName,
            Content = message,
            Type = MessageType.Text,
            Timestamp = DateTime.Now
        };

        _chatService.AddMessage(chatMessage);
        await Clients.All.SendAsync("ReceiveMessage", chatMessage);
    }

    public async Task SendImage(string userId, string userName, string imageData, string fileName)
    {
        var (success, filePath, error) = await _fileUploadService.SaveBase64FileAsync(imageData, fileName, "images");
        
        if (success && filePath != null)
        {
            var chatMessage = new ChatMessage
            {
                UserId = userId,
                UserName = userName,
                Content = "Image",
                Type = MessageType.Image,
                FileName = fileName,
                FileUrl = filePath,
                Timestamp = DateTime.Now
            };

            _chatService.AddMessage(chatMessage);
            await Clients.All.SendAsync("ReceiveMessage", chatMessage);
        }
        else
        {
            await Clients.Caller.SendAsync("ReceiveError", error ?? "Failed to upload image");
        }
    }

    public async Task SendVoiceMessage(string userId, string userName, string audioData, int duration)
    {
        var fileName = $"voice_{Guid.NewGuid()}.webm";
        var (success, filePath, error) = await _fileUploadService.SaveBase64FileAsync(audioData, fileName, "voice");
        
        if (success && filePath != null)
        {
            var chatMessage = new ChatMessage
            {
                UserId = userId,
                UserName = userName,
                Content = "Voice message",
                Type = MessageType.Voice,
                FileName = fileName,
                FileUrl = filePath,
                Duration = duration,
                Timestamp = DateTime.Now
            };

            _chatService.AddMessage(chatMessage);
            await Clients.All.SendAsync("ReceiveMessage", chatMessage);
        }
        else
        {
            await Clients.Caller.SendAsync("ReceiveError", error ?? "Failed to upload voice message");
        }
    }

    public async Task SendFile(string userId, string userName, string fileData, string fileName)
    {
        var (success, filePath, error) = await _fileUploadService.SaveBase64FileAsync(fileData, fileName, "files");
        
        if (success && filePath != null)
        {
            var chatMessage = new ChatMessage
            {
                UserId = userId,
                UserName = userName,
                Content = "File attachment",
                Type = MessageType.File,
                FileName = fileName,
                FileUrl = filePath,
                Timestamp = DateTime.Now
            };

            _chatService.AddMessage(chatMessage);
            await Clients.All.SendAsync("ReceiveMessage", chatMessage);
        }
        else
        {
            await Clients.Caller.SendAsync("ReceiveError", error ?? "Failed to upload file");
        }
    }

    public async Task UserTyping(string userName)
    {
        await Clients.Others.SendAsync("UserTyping", userName);
    }

    public async Task UserConnected(string userId, string userName)
    {
        var user = new User
        {
            Id = userId,
            Name = userName,
            IsOnline = true,
            LastSeen = DateTime.Now
        };

        _chatService.AddUser(user);
        
        // Send existing messages to the newly connected user
        var messages = _chatService.GetMessages();
        await Clients.Caller.SendAsync("LoadMessages", messages);
        
        // Notify all clients about the new user
        await Clients.All.SendAsync("UserConnected", user);
        
        // Send online users list to the newly connected user
        var onlineUsers = _chatService.GetOnlineUsers();
        await Clients.Caller.SendAsync("UpdateUserList", onlineUsers);
        
        // Notify others to update their user list
        await Clients.Others.SendAsync("UpdateUserList", onlineUsers);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.ConnectionId;
        _chatService.RemoveUser(userId);
        
        var user = _chatService.GetUser(userId);
        if (user != null)
        {
            await Clients.All.SendAsync("UserDisconnected", user);
        }
        
        var onlineUsers = _chatService.GetOnlineUsers();
        await Clients.All.SendAsync("UpdateUserList", onlineUsers);
        
        await base.OnDisconnectedAsync(exception);
    }
}
