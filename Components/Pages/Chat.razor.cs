using System.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using blazorchat.Models;

namespace blazorchat.Components.Pages;

public partial class Chat : ComponentBase, IAsyncDisposable
{
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    private HubConnection? hubConnection;
    private List<ChatMessage> messages = new();
    private List<User> onlineUsers = new();
    private string userName = "";
    private string userId = Guid.NewGuid().ToString();
    private bool isJoined = false;
    private string typingUser = "";
    private System.Threading.Timer? typingTimer;
    private string? selectedUserId;
    private string selectedChatName = "Select a contact";
    private bool isGroupChat = false;
    private Dictionary<string, int> unreadCounts = new();
    private const string RefreshWarningMessage = "Refreshing will disconnect you. Do you want to continue?";
    private bool hasAppliedStoredSelection = false;

    private sealed class StoredChatUser
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
    }

    private sealed class StoredChatSelection
    {
        public string? SelectedUserId { get; set; }
        public string SelectedChatName { get; set; } = "";
        public bool IsGroupChat { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        // Check for URL parameters (for WebForm integration)
        var uri = new Uri(Navigation.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);

        var userIdParam = query["userId"];
        var userNameParam = query["userName"];

        if (!string.IsNullOrEmpty(userIdParam))
        {
            userId = userIdParam;
        }

        if (!string.IsNullOrEmpty(userNameParam))
        {
            userName = userNameParam;
        }

        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/chathub"))
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
            .Build();

        // Handle reconnection events
        hubConnection.Reconnecting += error =>
        {
            Console.WriteLine("Connection lost. Attempting to reconnect...");
            InvokeAsync(StateHasChanged);
            return Task.CompletedTask;
        };

        hubConnection.Reconnected += async connectionId =>
        {
            Console.WriteLine("Reconnected successfully. Rejoining chat...");
            if (isJoined && !string.IsNullOrEmpty(userName))
            {
                // Rejoin the chat after reconnection
                await hubConnection.SendAsync("UserConnected", userId, userName);
            }
            InvokeAsync(StateHasChanged);
        };

        hubConnection.Closed += async error =>
        {
            Console.WriteLine($"Connection closed: {error?.Message}");
            // Try to restart after a delay
            await Task.Delay(5000);
            if (hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await hubConnection.StartAsync();
                    if (isJoined && !string.IsNullOrEmpty(userName))
                    {
                        await hubConnection.SendAsync("UserConnected", userId, userName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error restarting connection: {ex.Message}");
                }
            }
            InvokeAsync(StateHasChanged);
        };

        hubConnection.On<List<ChatMessage>>("LoadMessages", loadedMessages =>
        {
            messages = loadedMessages;
            UpdateUnreadCounts();
            _ = InvokeAsync(ApplyStoredSelectionIfReadyAsync);
            InvokeAsync(StateHasChanged);
        });

        hubConnection.On<ChatMessage>("ReceiveMessage", message =>
        {
            messages.Add(message);
            if (ShouldNotifyForMessage(message))
            {
                unreadCounts[message.SenderId] = unreadCounts.TryGetValue(message.SenderId, out var count)
                    ? count + 1
                    : 1;
            }
            else if (ShouldMarkAsRead(message))
            {
                _ = InvokeAsync(() => MarkMessagesAsRead(message.SenderId));
            }
            InvokeAsync(StateHasChanged);
            InvokeAsync(async () => await JSRuntime.InvokeVoidAsync("scrollToBottom"));
        });

        hubConnection.On<string>("UserConnected", user =>
        {
            InvokeAsync(StateHasChanged);
        });

        hubConnection.On<string>("UserDisconnected", user =>
        {
            InvokeAsync(StateHasChanged);
        });

        hubConnection.On<List<User>>("UpdateUserList", users =>
        {
            onlineUsers = users;
            _ = InvokeAsync(ApplyStoredSelectionIfReadyAsync);
            InvokeAsync(StateHasChanged);
        });

        hubConnection.On<string, string, string?, bool>("UserTyping", (senderId, senderName, recipientId, isGroup) =>
        {
            if (IsTypingForActiveChat(senderId, recipientId, isGroup))
            {
                typingUser = senderName;
                InvokeAsync(StateHasChanged);

                // Clear typing indicator after 3 seconds
                typingTimer?.Dispose();
                typingTimer = new System.Threading.Timer(_ =>
                {
                    typingUser = "";
                    InvokeAsync(StateHasChanged);
                }, null, 3000, Timeout.Infinite);
            }
        });

        await hubConnection.StartAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        await JSRuntime.InvokeVoidAsync("registerRefreshWarning", RefreshWarningMessage);

        var storedUser = await JSRuntime.InvokeAsync<StoredChatUser?>("chatStorage.loadUser");
        if (storedUser is not null
            && !string.IsNullOrWhiteSpace(storedUser.UserId)
            && !string.IsNullOrWhiteSpace(storedUser.UserName))
        {
            userId = storedUser.UserId;
            userName = storedUser.UserName;
            await JoinChat();
            await ApplyStoredSelectionIfReadyAsync();
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task JoinChat()
    {
        if (!string.IsNullOrWhiteSpace(userName) && hubConnection is not null)
        {
            isJoined = true;
            await hubConnection.SendAsync("UserConnected", userId, userName);
            await JSRuntime.InvokeVoidAsync("chatStorage.saveUser", new StoredChatUser
            {
                UserId = userId,
                UserName = userName
            });
        }
    }

    private async Task SendMessage(string message)
    {
        if (hubConnection is not null && !string.IsNullOrWhiteSpace(message) && canSendMessages)
        {
            await hubConnection.SendAsync("SendMessage", userId, userName, message, selectedUserId, isGroupChat);
        }
    }

    private async Task SendImage((string imageData, string fileName) data)
    {
        if (hubConnection is not null && hubConnection.State == HubConnectionState.Connected && canSendMessages)
        {
            try
            {
                await hubConnection.SendAsync("SendImage", userId, userName, data.imageData, data.fileName, selectedUserId, isGroupChat);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending image: {ex.Message}");
            }
        }
    }

    private async Task SendVoiceMessage((string audioData, int duration) data)
    {
        if (hubConnection is not null && hubConnection.State == HubConnectionState.Connected && canSendMessages)
        {
            try
            {
                await hubConnection.SendAsync("SendVoiceMessage", userId, userName, data.audioData, data.duration, selectedUserId, isGroupChat);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending voice message: {ex.Message}");
            }
        }
    }

    private async Task SendFile((string fileData, string fileName) data)
    {
        if (hubConnection is not null && hubConnection.State == HubConnectionState.Connected && canSendMessages)
        {
            try
            {
                await hubConnection.SendAsync("SendFile", userId, userName, data.fileData, data.fileName, selectedUserId, isGroupChat);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending file: {ex.Message}");
            }
        }
    }

    private async Task HandleTyping()
    {
        if (hubConnection is not null && canSendMessages)
        {
            await hubConnection.SendAsync("UserTyping", userId, userName, selectedUserId, isGroupChat);
        }
    }

    private async Task SelectUser(string selectedId)
    {
        var selectedUser = onlineUsers.FirstOrDefault(user => user.Id == selectedId);
        if (selectedUser is null)
        {
            return;
        }

        selectedUserId = selectedUser.Id;
        selectedChatName = selectedUser.Name;
        isGroupChat = false;
        typingUser = "";
        await PersistSelection();
        await MarkMessagesAsRead(selectedUser.Id);
    }

    private async Task SelectGroup()
    {
        selectedUserId = null;
        selectedChatName = "Group Chat";
        isGroupChat = true;
        typingUser = "";
        await PersistSelection();
    }

    private bool IsTypingForActiveChat(string senderId, string? recipientId, bool isGroup)
    {
        if (isGroupChat && isGroup)
        {
            return true;
        }

        if (!isGroupChat && !isGroup && !string.IsNullOrWhiteSpace(selectedUserId))
        {
            return senderId == selectedUserId && recipientId == userId;
        }

        return false;
    }

    private bool ShouldNotifyForMessage(ChatMessage message)
    {
        if (message.IsGroup || message.RecipientId != userId || message.SenderId == userId)
        {
            return false;
        }

        return isGroupChat || selectedUserId != message.SenderId;
    }

    private bool ShouldMarkAsRead(ChatMessage message)
    {
        return !message.IsGroup
            && message.RecipientId == userId
            && message.SenderId != userId
            && !isGroupChat
            && selectedUserId == message.SenderId;
    }

    private void UpdateUnreadCounts()
    {
        unreadCounts = messages
            .Where(message => !message.IsGroup
                && message.RecipientId == userId
                && !message.IsRead
                && message.SenderId != userId)
            .GroupBy(message => message.SenderId)
            .ToDictionary(group => group.Key, group => group.Count());
    }

    private async Task MarkMessagesAsRead(string senderId)
    {
        var unreadMessages = messages
            .Where(message => !message.IsGroup
                && message.RecipientId == userId
                && message.SenderId == senderId
                && !message.IsRead)
            .ToList();

        if (unreadMessages.Count == 0)
        {
            return;
        }

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        unreadCounts.Remove(senderId);
        await InvokeAsync(StateHasChanged);

        if (hubConnection is not null)
        {
            foreach (var message in unreadMessages)
            {
                await hubConnection.SendAsync("MessageRead", message.Id);
            }
        }
    }

    private List<ChatMessage> filteredMessages =>
        isGroupChat
            ? messages.Where(message => message.IsGroup).ToList()
            : messages
                .Where(message => !message.IsGroup
                    && !string.IsNullOrWhiteSpace(selectedUserId)
                    && ((message.SenderId == userId && message.RecipientId == selectedUserId)
                        || (message.SenderId == selectedUserId && message.RecipientId == userId)))
                .ToList();

    private bool canSendMessages => isGroupChat || !string.IsNullOrWhiteSpace(selectedUserId);

    public async ValueTask DisposeAsync()
    {
        typingTimer?.Dispose();
        await JSRuntime.InvokeVoidAsync("clearRefreshWarning");
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }

    private async Task PersistSelection()
    {
        await JSRuntime.InvokeVoidAsync("chatStorage.saveSelection", new StoredChatSelection
        {
            SelectedUserId = selectedUserId,
            SelectedChatName = selectedChatName,
            IsGroupChat = isGroupChat
        });
    }

    private async Task ApplyStoredSelectionIfReadyAsync()
    {
        if (hasAppliedStoredSelection || !isJoined)
        {
            return;
        }

        var storedSelection = await JSRuntime.InvokeAsync<StoredChatSelection?>("chatStorage.loadSelection");

        if (storedSelection is null)
        {
            hasAppliedStoredSelection = true;
            return;
        }

        if (storedSelection.IsGroupChat)
        {
            selectedUserId = null;
            selectedChatName = "Group Chat";
            isGroupChat = true;
            typingUser = "";
        }
        else if (!string.IsNullOrWhiteSpace(storedSelection.SelectedUserId))
        {
            var selectedUser = onlineUsers.FirstOrDefault(user => user.Id == storedSelection.SelectedUserId);
            if (selectedUser is not null)
            {
                selectedUserId = selectedUser.Id;
                selectedChatName = string.IsNullOrWhiteSpace(storedSelection.SelectedChatName)
                    ? selectedUser.Name
                    : storedSelection.SelectedChatName;
                isGroupChat = false;
                typingUser = "";
                _ = MarkMessagesAsRead(selectedUser.Id);
            }
        }

        hasAppliedStoredSelection = true;
    }
}
