using blazorchat.Data;
using blazorchat.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace blazorchat.Services;

public class ChatService : IChatService
{
    private readonly IDbContextFactory<ChatDbContext> _dbContextFactory;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, User> _userSessions = new();

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

    // Database user operations
    public async Task<User?> GetUserByNameAsync(string userName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Name == userName);
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User> CreateUserAsync(string userName)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = userName,
            CreatedAt = DateTime.UtcNow,
            LastSeen = DateTime.UtcNow
        };
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    public async Task UpdateUserLastSeenAsync(string userId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            user.LastSeen = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }
    }

    // In-memory session management
    public void AddUserSession(User user)
    {
        _userSessions.TryAdd(user.Id, user);
    }

    public void RemoveUserSession(string userId)
    {
        _userSessions.TryRemove(userId, out _);
    }

    public List<User> GetOnlineUsers()
    {
        return _userSessions.Values.Where(u => u.IsOnline).ToList();
    }

    public User? GetUserByConnectionId(string connectionId)
    {
        return _userSessions.Values.FirstOrDefault(u => u.ConnectionId == connectionId);
    }

    public User? GetUserSessionById(string userId)
    {
        _userSessions.TryGetValue(userId, out var user);
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

    public async Task<ChatMessage?> GetMessageByIdAsync(string messageId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.ChatMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == messageId);
    }

    public async Task DeleteMessageAsync(string messageId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var message = await dbContext.ChatMessages.FirstOrDefaultAsync(m => m.Id == messageId);
        if (message is not null)
        {
            dbContext.ChatMessages.Remove(message);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteChatAsync(string userId, string? otherUserId, bool isGroup)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        IQueryable<ChatMessage> query = dbContext.ChatMessages;
        
        if (isGroup)
        {
            // Delete all group messages
            query = query.Where(m => m.IsGroup);
        }
        else if (!string.IsNullOrWhiteSpace(otherUserId))
        {
            // Delete all messages between two users
            query = query.Where(m => !m.IsGroup &&
                ((m.SenderId == userId && m.RecipientId == otherUserId) ||
                 (m.SenderId == otherUserId && m.RecipientId == userId)));
        }
        else
        {
            return; // Invalid parameters
        }

        var messagesToDelete = await query.ToListAsync();
        dbContext.ChatMessages.RemoveRange(messagesToDelete);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var allUsers = await dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();
        
        // Mark users as online based on active sessions
        foreach (var user in allUsers)
        {
            if (_userSessions.TryGetValue(user.Id, out var sessionUser))
            {
                user.IsOnline = true;
                user.ConnectionId = sessionUser.ConnectionId;
            }
            else
            {
                // Check if user was active within last 5 minutes
                user.IsOnline = (DateTime.UtcNow - user.LastSeen).TotalMinutes < 5;
            }
        }
        
        return allUsers;
    }
}
