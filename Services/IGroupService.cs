using blazorchat.Models;

namespace blazorchat.Services;

public interface IGroupService
{
    Task<(bool Success, string Message, Group? Group)> CreateGroupAsync(string name, string description, string createdBy);
    Task<bool> AddMemberAsync(string groupId, string userId, bool isAdmin = false);
    Task<bool> RemoveMemberAsync(string groupId, string userId);
    Task<bool> UpdateGroupAsync(string groupId, string name, string? description, string? avatarUrl);
    Task<bool> DeleteGroupAsync(string groupId, string userId);
    Task<List<Group>> GetUserGroupsAsync(string userId);
    Task<Group?> GetGroupByIdAsync(string groupId);
    Task<List<User>> GetGroupMembersAsync(string groupId);
    Task<bool> IsUserMemberAsync(string groupId, string userId);
    Task<bool> IsUserAdminAsync(string groupId, string userId);
}
