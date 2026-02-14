using blazorchat.Models;

namespace blazorchat.Services;

public interface IChatService
{
    Task AddMessageAsync(ChatMessage message);
    Task<List<ChatMessage>> GetMessagesAsync();
    Task<List<ChatMessage>> GetMessagesForUserAsync(string userId);
    
    // Database user operations
    Task<User?> GetUserByNameAsync(string userName);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User> CreateUserAsync(string userName);
    Task UpdateUserLastSeenAsync(string userId);
    
    // In-memory session management
    void AddUserSession(User user);
    void RemoveUserSession(string userId);
    List<User> GetOnlineUsers();
    User? GetUserByConnectionId(string connectionId);
    User? GetUserSessionById(string userId);
    Task<List<User>> GetAllUsersAsync();
    
    Task MarkMessageAsReadAsync(string messageId);
    Task<ChatMessage?> GetMessageByIdAsync(string messageId);
    Task DeleteMessageAsync(string messageId);
    Task DeleteChatAsync(string userId, string? otherUserId, bool isGroup);
}
