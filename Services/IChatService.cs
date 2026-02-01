using blazorchat.Models;

namespace blazorchat.Services;

public interface IChatService
{
    void AddMessage(ChatMessage message);
    List<ChatMessage> GetMessages();
    void AddUser(User user);
    void RemoveUser(string userId);
    List<User> GetOnlineUsers();
    User? GetUserByConnectionId(string connectionId);
    void MarkMessageAsRead(string messageId);
}
