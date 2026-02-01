using blazorchat.Models;
using System.Collections.Concurrent;

namespace blazorchat.Services;

public class ChatService : IChatService
{
    private readonly ConcurrentBag<ChatMessage> _messages = new();
    private readonly ConcurrentDictionary<string, User> _users = new();

    public void AddMessage(ChatMessage message)
    {
        _messages.Add(message);
    }

    public List<ChatMessage> GetMessages()
    {
        return _messages.OrderBy(m => m.Timestamp).ToList();
    }

    public void AddUser(User user)
    {
        _users.TryAdd(user.Id, user);
    }

    public void RemoveUser(string userId)
    {
        _users.TryRemove(userId, out _);
    }

    public List<User> GetOnlineUsers()
    {
        return _users.Values.Where(u => u.IsOnline).ToList();
    }

    public User? GetUserByConnectionId(string connectionId)
    {
        return _users.Values.FirstOrDefault(u => u.ConnectionId == connectionId);
    }

    public void MarkMessageAsRead(string messageId)
    {
        var message = _messages.FirstOrDefault(m => m.Id == messageId);
        if (message != null)
        {
            message.IsRead = true;
        }
    }
}
