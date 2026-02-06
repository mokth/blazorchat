using blazorchat.Data;
using blazorchat.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace blazorchat.Services;

public class ChatService : IChatService
{
    private readonly IDbContextFactory<ChatDbContext> _dbContextFactory;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, User> _users = new();

    public ChatService(IDbContextFactory<ChatDbContext> dbContextFactory, IConfiguration configuration)
    {
        _dbContextFactory = dbContextFactory;
        _configuration = configuration;
    }

    public async Task AddMessageAsync(ChatMessage message)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        dbContext.ChatMessages.Add(message);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<ChatMessage>> GetMessagesAsync()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ChatMessages
            .AsNoTracking()
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<List<ChatMessage>> GetMessagesForUserAsync(string userId)
    {
        var historyDays = _configuration.GetValue<int>("ChatRetention:HistoryDays");
        var cutoff = DateTime.UtcNow.AddDays(-historyDays);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ChatMessages
            .AsNoTracking()
            .Where(message => message.Timestamp >= cutoff
                && (message.IsGroup
                    || message.SenderId == userId
                    || message.RecipientId == userId))
            .OrderBy(message => message.Timestamp)
            .ToListAsync();
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

    public User? GetUserById(string userId)
    {
        _users.TryGetValue(userId, out var user);
        return user;
    }

    public async Task MarkMessageAsReadAsync(string messageId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var message = await dbContext.ChatMessages.FirstOrDefaultAsync(m => m.Id == messageId);
        if (message is not null)
        {
            message.IsRead = true;
            await dbContext.SaveChangesAsync();
        }
    }
}
