using blazorchat.Models;
using System.Collections.Concurrent;

namespace blazorchat.Services;

public class ChatService : IChatService
{
    private readonly ConcurrentDictionary<string, User> _users = new();
    private readonly List<ChatMessage> _messages = new();
    private readonly object _messagesLock = new();

    public User ConnectUser(string userName, string connectionId)
    {
        var user = new User
        {
            Name = userName,
            ConnectionId = connectionId,
            IsOnline = true,
            LastSeen = DateTime.Now
        };

        _users.AddOrUpdate(userName, user, (key, existingUser) =>
        {
            existingUser.ConnectionId = connectionId;
            existingUser.IsOnline = true;
            existingUser.LastSeen = DateTime.Now;
            return existingUser;
        });

        return user;
    }

    public void SetUserOffline(string userName)
    {
        if (_users.TryGetValue(userName, out var user))
        {
            user.IsOnline = false;
            user.LastSeen = DateTime.Now;
        }
    }

    public User? GetUserByConnectionId(string connectionId)
    {
        return _users.Values.FirstOrDefault(u => u.ConnectionId == connectionId);
    }

    public List<User> GetOnlineUsers()
    {
        return _users.Values.Where(u => u.IsOnline).ToList();
    }

    public void AddMessage(ChatMessage message)
    {
        lock (_messagesLock)
        {
            _messages.Add(message);
        }
    }

    public List<ChatMessage> GetMessages()
    {
        lock (_messagesLock)
        {
            return new List<ChatMessage>(_messages);
        }
    }

    public void MarkMessageAsRead(string messageId)
    {
        lock (_messagesLock)
        {
            var message = _messages.FirstOrDefault(m => m.Id == messageId);
            if (message != null)
            {
                message.IsRead = true;
            }
        }
    }
}
