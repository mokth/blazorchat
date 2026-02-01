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
            _chatService.SetUserOffline(user.Name);
            await Clients.All.SendAsync("UserDisconnected", user.Name);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task UserConnected(string userName)
    {
        var user = _chatService.ConnectUser(userName, Context.ConnectionId);
        await Clients.All.SendAsync("UserConnected", userName);
        
        // Send message history to the newly connected user
        var messages = _chatService.GetMessages();
        await Clients.Caller.SendAsync("LoadMessages", messages);
        
        // Send user list to all clients
        var users = _chatService.GetOnlineUsers();
        await Clients.All.SendAsync("UpdateUserList", users);
    }

    public async Task SendMessage(string user, string message)
    {
        var chatMessage = new ChatMessage
        {
            User = user,
            Content = message,
            Type = MessageType.Text,
            Timestamp = DateTime.Now
        };
        
        _chatService.AddMessage(chatMessage);
        await Clients.All.SendAsync("ReceiveMessage", chatMessage);
    }

    public async Task SendImage(string user, string imageData, string fileName)
    {
        var chatMessage = new ChatMessage
        {
            User = user,
            Content = imageData,
            Type = MessageType.Image,
            FileName = fileName,
            Timestamp = DateTime.Now
        };
        
        _chatService.AddMessage(chatMessage);
        await Clients.All.SendAsync("ReceiveMessage", chatMessage);
    }

    public async Task SendVoiceMessage(string user, string audioData, int duration)
    {
        var chatMessage = new ChatMessage
        {
            User = user,
            Content = audioData,
            Type = MessageType.Voice,
            Duration = duration,
            Timestamp = DateTime.Now
        };
        
        _chatService.AddMessage(chatMessage);
        await Clients.All.SendAsync("ReceiveMessage", chatMessage);
    }

    public async Task SendFile(string user, string fileData, string fileName)
    {
        var chatMessage = new ChatMessage
        {
            User = user,
            Content = fileData,
            Type = MessageType.File,
            FileName = fileName,
            Timestamp = DateTime.Now
        };
        
        _chatService.AddMessage(chatMessage);
        await Clients.All.SendAsync("ReceiveMessage", chatMessage);
    }

    public async Task UserTyping(string user)
    {
        await Clients.Others.SendAsync("UserTyping", user);
    }

    public async Task MarkMessageAsRead(string messageId)
    {
        _chatService.MarkMessageAsRead(messageId);
        await Clients.All.SendAsync("MessageRead", messageId);
    }
}
