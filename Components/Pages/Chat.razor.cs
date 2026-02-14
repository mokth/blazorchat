using System.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using blazorchat.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace blazorchat.Components.Pages;

public partial class Chat : ComponentBase, IAsyncDisposable
{
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] public ProtectedSessionStorage SessionStorage { get; set; } = default!;

    private HubConnection? hubConnection;
    private List<ChatMessage> messages = new();
    private List<User> onlineUsers = new();
    private string userName = "";
    private string userId = "";
    private bool isJoined = false;
    private string typingUser = "";
    private System.Threading.Timer? typingTimer;
    private string? selectedUserId;
    private string selectedChatName = "Select a contact";
    private bool isGroupChat = false;
    private Dictionary<string, int> unreadCounts = new();
    private const string RefreshWarningMessage = "Refreshing will disconnect you. Do you want to continue?";
    private bool hasRegisteredRefreshWarning = false;
    private bool isAuthenticated = false;
    private bool isDisposing = false;

    protected override async Task OnInitializedAsync()
    {
        // Check for URL parameters (for WebForm integration)
        // Try to get authenticated user from session
        try
        {
            var userIdResult = await SessionStorage.GetAsync<string>("userId");
            var userNameResult = await SessionStorage.GetAsync<string>("userName");
            
            if (userIdResult.Success && !string.IsNullOrEmpty(userIdResult.Value) &&
                userNameResult.Success && !string.IsNullOrEmpty(userNameResult.Value))
            {
                isAuthenticated = true;
                userId = userIdResult.Value;
                userName = userNameResult.Value;
            }
        }
        catch
        {
            // Session storage not available yet
        }
    }

    private async Task InitConnection()
    {
        var uri = new Uri(Navigation.Uri);
        var query = HttpUtility.ParseQueryString(uri.Query);

        var userNameParam = query["userName"];

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
                await hubConnection.SendAsync("UserConnected", userName);
            }
           await InvokeAsync(StateHasChanged);
        };

        hubConnection.Closed += async error =>
        {
            if (isDisposing)
            {
                return;
            }

            Console.WriteLine($"Connection closed: {error?.Message}");
            // Try to restart after a delay
            await Task.Delay(5000);
            if (!isDisposing && hubConnection != null && hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    await hubConnection.StartAsync();
                    if (isJoined && !string.IsNullOrEmpty(userName))
                    {
                        await hubConnection.SendAsync("UserConnected", userName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error restarting connection: {ex.Message}");
                }
            }
            await InvokeAsync(StateHasChanged);
        };

        hubConnection.On<string>("UserIdAssigned", assignedUserId =>
        {
            userId = assignedUserId;
            Console.WriteLine($"User ID assigned: {userId}");
            InvokeAsync(StateHasChanged);
        });

        hubConnection.On<List<ChatMessage>>("LoadMessages", loadedMessages =>
        {
            messages = loadedMessages;
            UpdateUnreadCounts();
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

        hubConnection.On<string>("MessageDeleted", messageId =>
        {
            var message = messages.FirstOrDefault(m => m.Id == messageId);
            if (message != null)
            {
                messages.Remove(message);
                InvokeAsync(StateHasChanged);
            }
        });

        hubConnection.On<string, string?, bool>("ChatCleared", (senderId, otherUserId, isGroup) =>
        {
            if (isGroup && isGroupChat)
            {
                messages.RemoveAll(m => m.IsGroup);
                InvokeAsync(StateHasChanged);
            }
            else if (!isGroup && !string.IsNullOrWhiteSpace(otherUserId))
            {
                if (selectedUserId == otherUserId || userId == otherUserId)
                {
                    messages.RemoveAll(m => !m.IsGroup &&
                        ((m.SenderId == userId && m.RecipientId == otherUserId) ||
                         (m.SenderId == otherUserId && m.RecipientId == userId)));
                    InvokeAsync(StateHasChanged);
                }
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
        await InitConnection();
        await JSRuntime.InvokeVoidAsync("registerRefreshWarning", RefreshWarningMessage);
        hasRegisteredRefreshWarning = true;
        
        // Auto-join authenticated users to chat
        if (isAuthenticated && !isJoined)
        {
            await JoinChat();
        }
    }

    private async Task JoinChat()
    {
        if (!string.IsNullOrWhiteSpace(userName) && hubConnection is not null)
        {
            isJoined = true;
            await hubConnection.SendAsync("UserConnected", userName);
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
        await MarkMessagesAsRead(selectedUser.Id);
        await InvokeAsync(StateHasChanged);
        await Task.Delay(100); // Wait for UI to render
        await JSRuntime.InvokeVoidAsync("scrollToBottom");
    }

    private async Task SelectGroup()
    {
        selectedUserId = null;
        selectedChatName = "Group Chat";
        isGroupChat = true;
        typingUser = "";
        await InvokeAsync(StateHasChanged);
        await Task.Delay(100); // Wait for UI to render
        await JSRuntime.InvokeVoidAsync("scrollToBottom");
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
        isDisposing = true;
        typingTimer?.Dispose();

        // During prerender/static rendering, JS interop isn't available. Only clear the warning
        // if we successfully registered it after the first interactive render.
        if (hasRegisteredRefreshWarning)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("clearRefreshWarning");
            }
            catch (JSDisconnectedException)
            {
                // Ignore: circuit already disconnected during refresh/navigation.
            }
            catch (InvalidOperationException)
            {
                // Ignore: JS runtime not available (e.g., circuit already gone).
            }
        }

        if (hubConnection is not null)
        {
            try
            {
                await hubConnection.DisposeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing hub connection: {ex.Message}");
            }
        }
    }

    private async Task HandleReplyMessage((string messageId, string replyToId) data)
    {
        var replyToMessage = messages.FirstOrDefault(m => m.Id == data.replyToId);
        if (replyToMessage == null || hubConnection is null)
        {
            return;
        }

        // Note: data.messageId actually contains the message content (text), not an ID
        // This is passed from ChatWindow.HandleSendMessage when replying
        // data.replyToId is the ID of the message being replied to
        await hubConnection.SendAsync("ReplyMessage", userId, userName, data.messageId, selectedUserId, isGroupChat, data.replyToId);
    }

    private async Task HandleForwardMessage((string messageId, string? recipientId, bool isGroup) data)
    {
        if (hubConnection is null)
        {
            return;
        }

        await hubConnection.SendAsync("ForwardMessage", userId, userName, data.messageId, data.recipientId, data.isGroup);
    }

    private async Task HandleDeleteMessage(string messageId)
    {
        if (hubConnection is null)
        {
            return;
        }

        await hubConnection.SendAsync("DeleteMessage", messageId, userId);
    }

    private async Task HandleClearChat()
    {
        if (hubConnection is null)
        {
            return;
        }

        await hubConnection.SendAsync("ClearChat", userId, selectedUserId, isGroupChat);
    }
}
