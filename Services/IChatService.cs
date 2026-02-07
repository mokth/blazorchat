using blazorchat.Models;

namespace blazorchat.Services;

public interface IChatService
{
    Task AddMessageAsync(ChatMessage message);
    Task<List<ChatMessage>> GetMessagesAsync();
    Task<List<ChatMessage>> GetMessagesForUserAsync(string userId);
    void AddUser(User user);
    void RemoveUser(string userId);
    List<User> GetOnlineUsers();
    User? GetUserByConnectionId(string connectionId);
    User? GetUserById(string userId);
    Task MarkMessageAsReadAsync(string messageId);
    Task<ChatMessage?> GetMessageByIdAsync(string messageId);
    Task DeleteMessageAsync(string messageId);
    Task DeleteChatAsync(string userId, string? otherUserId, bool isGroup);
}
