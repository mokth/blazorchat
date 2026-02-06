using blazorchat.Models;

namespace blazorchat.Services;

public interface IChatService
{
    void AddMessage(ChatMessage message);
    List<ChatMessage> GetMessages();
    List<ChatMessage> GetMessagesForUser(string userId);
    void AddUser(User user);
    void RemoveUser(string userId);
    List<User> GetOnlineUsers();
    User? GetUserByConnectionId(string connectionId);
    User? GetUserById(string userId);
    void MarkMessageAsRead(string messageId);
}
