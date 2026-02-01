using blazorchat.Models;

namespace blazorchat.Services;

public interface IChatService
{
    User ConnectUser(string userName, string connectionId);
    void SetUserOffline(string userName);
    User? GetUserByConnectionId(string connectionId);
    List<User> GetOnlineUsers();
    void AddMessage(ChatMessage message);
    List<ChatMessage> GetMessages();
    void MarkMessageAsRead(string messageId);
}
