using blazorchat.Models;
using System.Collections.Concurrent;

namespace blazorchat.Services;

public class ChatService : IChatService
{
    private readonly ConcurrentBag<ChatMessage> _messages = new();
    private readonly ConcurrentDictionary<string, User> _users = new();

    public event Action<ChatMessage>? OnMessageReceived;
    public event Action<User>? OnUserConnected;
    public event Action<User>? OnUserDisconnected;
    public event Action<string>? OnUserTyping;

    public List<ChatMessage> GetMessages()
    {
        return _messages.OrderBy(m => m.Timestamp).ToList();
    }

    public List<User> GetOnlineUsers()
    {
        return _users.Values.Where(u => u.IsOnline).OrderBy(u => u.Name).ToList();
    }

    public void AddMessage(ChatMessage message)
    {
        _messages.Add(message);
        OnMessageReceived?.Invoke(message);
    }

    public void AddUser(User user)
    {
        _users[user.Id] = user;
        OnUserConnected?.Invoke(user);
    }

    public void RemoveUser(string userId)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            user.IsOnline = false;
            user.LastSeen = DateTime.Now;
            OnUserDisconnected?.Invoke(user);
        }
    }

    public User? GetUser(string userId)
    {
        _users.TryGetValue(userId, out var user);
        return user;
    }

    public void UpdateUserStatus(string userId, bool isOnline)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            user.IsOnline = isOnline;
            if (!isOnline)
            {
                user.LastSeen = DateTime.Now;
            }
        }
    }

    public void NotifyUserTyping(string userName)
    {
        OnUserTyping?.Invoke(userName);
    }
}
