using blazorchat.Data;
using blazorchat.Models;
using Microsoft.EntityFrameworkCore;

namespace blazorchat.Services;

public class GroupService : IGroupService
{
    private readonly IDbContextFactory<ChatDbContext> _dbContextFactory;

    public GroupService(IDbContextFactory<ChatDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<(bool Success, string Message, Group? Group)> CreateGroupAsync(string name, string description, string createdBy)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        // Check if group name already exists
        var existingGroup = await context.Groups.FirstOrDefaultAsync(g => g.Name == name);
        if (existingGroup != null)
        {
            return (false, "Group name already exists", null);
        }

        var group = new Group
        {
            Name = name,
            Description = description,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        context.Groups.Add(group);

        // Add creator as admin member
        var member = new GroupMember
        {
            GroupId = group.Id,
            UserId = createdBy,
            IsAdmin = true,
            JoinedAt = DateTime.UtcNow
        };

        context.GroupMembers.Add(member);
        await context.SaveChangesAsync();

        return (true, "Group created successfully", group);
    }

    public async Task<bool> AddMemberAsync(string groupId, string userId, bool isAdmin = false)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        // Check if member already exists
        var existingMember = await context.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
        
        if (existingMember != null)
        {
            return false;
        }

        var member = new GroupMember
        {
            GroupId = groupId,
            UserId = userId,
            IsAdmin = isAdmin,
            JoinedAt = DateTime.UtcNow
        };

        context.GroupMembers.Add(member);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveMemberAsync(string groupId, string userId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var member = await context.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
        
        if (member == null)
        {
            return false;
        }

        context.GroupMembers.Remove(member);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateGroupAsync(string groupId, string name, string? description, string? avatarUrl)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var group = await context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null)
        {
            return false;
        }

        // Check if new name is taken by another group
        if (group.Name != name)
        {
            var existingName = await context.Groups.FirstOrDefaultAsync(g => g.Name == name && g.Id != groupId);
            if (existingName != null)
            {
                return false;
            }
            group.Name = name;
        }

        group.Description = description;
        group.AvatarUrl = avatarUrl;
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteGroupAsync(string groupId, string userId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var group = await context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null || group.CreatedBy != userId)
        {
            return false;
        }

        context.Groups.Remove(group);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<List<Group>> GetUserGroupsAsync(string userId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        return await context.GroupMembers
            .Where(m => m.UserId == userId)
            .Include(m => m.Group)
            .Select(m => m.Group!)
            .ToListAsync();
    }

    public async Task<Group?> GetGroupByIdAsync(string groupId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);
    }

    public async Task<List<User>> GetGroupMembersAsync(string groupId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        return await context.GroupMembers
            .Where(m => m.GroupId == groupId)
            .Include(m => m.User)
            .Select(m => m.User!)
            .ToListAsync();
    }

    public async Task<bool> IsUserMemberAsync(string groupId, string userId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        return await context.GroupMembers
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
    }

    public async Task<bool> IsUserAdminAsync(string groupId, string userId)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var member = await context.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

        return member?.IsAdmin ?? false;
    }
}
