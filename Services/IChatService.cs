using blazorchat.Models;

namespace blazorchat.Services;

public interface IChatService
{
    event Action<ChatMessage>? OnMessageReceived;
    event Action<User>? OnUserConnected;
    event Action<User>? OnUserDisconnected;
    event Action<string>? OnUserTyping;
    
    List<ChatMessage> GetMessages();
    List<User> GetOnlineUsers();
    void AddMessage(ChatMessage message);
    void AddUser(User user);
    void RemoveUser(string userId);
    User? GetUser(string userId);
    void UpdateUserStatus(string userId, bool isOnline);
}
