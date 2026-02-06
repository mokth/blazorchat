using Microsoft.AspNetCore.Components;
using blazorchat.Models;

namespace blazorchat.Components.Shared;

public partial class UserList : ComponentBase
{
    [Parameter] public List<User> Users { get; set; } = new();
    [Parameter] public string CurrentUserId { get; set; } = "";
    [Parameter] public string? SelectedUserId { get; set; }
    [Parameter] public bool IsGroupSelected { get; set; }
    [Parameter] public Dictionary<string, int> UnreadCounts { get; set; } = new();
    [Parameter] public EventCallback<string> OnSelectUser { get; set; }
    [Parameter] public EventCallback OnSelectGroup { get; set; }
}
