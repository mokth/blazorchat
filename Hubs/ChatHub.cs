using Microsoft.AspNetCore.SignalR;
using blazorchat.Models;
using blazorchat.Services;

namespace blazorchat.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly FileUploadService _fileUploadService;
    private readonly IGroupService _groupService;

    public ChatHub(IChatService chatService, FileUploadService fileUploadService, IGroupService groupService)
    {
        _chatService = chatService;
        _fileUploadService = fileUploadService;
        _groupService = groupService;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = _chatService.GetUserByConnectionId(Context.ConnectionId);
        if (user != null)
        {
            _chatService.RemoveUserSession(user.Id);
            await _chatService.UpdateUserLastSeenAsync(user.Id);
            await Clients.All.SendAsync("UserDisconnected", user.Name);
            await SendUpdatedUserLists();
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task UserConnected(string userName)
    {
        // Look up user in database by name
        var dbUser = await _chatService.GetUserByNameAsync(userName);
        
        // If user doesn't exist, create new user in database
        if (dbUser == null)
        {
            dbUser = await _chatService.CreateUserAsync(userName);
        }
        else
        {
            // Update last seen for existing user
            await _chatService.UpdateUserLastSeenAsync(dbUser.Id);
        }
        
        // Create session user with connection info
        var sessionUser = new User
        {
            Id = dbUser.Id,
            Name = dbUser.Name,
            ConnectionId = Context.ConnectionId,
            IsOnline = true,
            LastSeen = DateTime.UtcNow,
            AvatarUrl = dbUser.AvatarUrl
        };

        _chatService.AddUserSession(sessionUser);
        
        // Send existing messages to the user
        var messages = await _chatService.GetMessagesForUserAsync(dbUser.Id);
        await Clients.Caller.SendAsync("LoadMessages", messages);
        
        // Send user ID back to client
        await Clients.Caller.SendAsync("UserIdAssigned", dbUser.Id);
        
        // Notify all users about the new connection
        await Clients.All.SendAsync("UserConnected", userName);
        
        // Send updated user list to all clients
        await SendUpdatedUserLists();
    }

    public async Task SendMessage(string senderId, string senderName, string message, string? recipientId, bool isGroup)
    {
        var chatMessage = new ChatMessage
        {
            User = senderName,
            SenderId = senderId,
            RecipientId = recipientId,
            IsGroup = isGroup,
            Content = message,
            Type = MessageType.Text,
            Timestamp = DateTime.UtcNow
        };

        await _chatService.AddMessageAsync(chatMessage);
        
        if (isGroup)
        {
            await SendGroupMessage(chatMessage, senderId, recipientId);
            return;
        }

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return;
        }

        var recipient = _chatService.GetUserSessionById(recipientId);
        if (recipient != null)
        {
            await Clients.Clients(Context.ConnectionId, recipient.ConnectionId)
                .SendAsync("ReceiveMessage", chatMessage);
        }
    }

    public async Task SendImage(string senderId, string senderName, string imageData, string fileName, string? recipientId, bool isGroup)
    {
        var fileUrl = await _fileUploadService.SaveFileAsync(imageData, fileName, MessageType.Image);
        var chatMessage = new ChatMessage
        {
            User = senderName,
            SenderId = senderId,
            RecipientId = recipientId,
            IsGroup = isGroup,
            Content = fileUrl,
            FileUrl = fileUrl,
            Type = MessageType.Image,
            FileName = fileName,
            Timestamp = DateTime.UtcNow
        };

        await _chatService.AddMessageAsync(chatMessage);
        if (isGroup)
        {
            await SendGroupMessage(chatMessage, senderId, recipientId);
            return;
        }

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return;
        }

        var recipient = _chatService.GetUserSessionById(recipientId);
        if (recipient != null)
        {
            await Clients.Clients(Context.ConnectionId, recipient.ConnectionId)
                .SendAsync("ReceiveMessage", chatMessage);
        }
    }

    public async Task SendVoiceMessage(string senderId, string senderName, string audioData, int duration, string? recipientId, bool isGroup)
    {
        var voiceFileName = $"voice_{Guid.NewGuid()}.webm";
        var fileUrl = await _fileUploadService.SaveFileAsync(audioData, voiceFileName, MessageType.Voice);
        var chatMessage = new ChatMessage
        {
            User = senderName,
            SenderId = senderId,
            RecipientId = recipientId,
            IsGroup = isGroup,
            Content = fileUrl,
            FileUrl = fileUrl,
            Type = MessageType.Voice,
            Duration = duration,
            Timestamp = DateTime.UtcNow
        };

        await _chatService.AddMessageAsync(chatMessage);
        if (isGroup)
        {
            await SendGroupMessage(chatMessage, senderId, recipientId);
            return;
        }

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return;
        }

        var recipient = _chatService.GetUserSessionById(recipientId);
        if (recipient != null)
        {
            await Clients.Clients(Context.ConnectionId, recipient.ConnectionId)
                .SendAsync("ReceiveMessage", chatMessage);
        }
    }

    public async Task SendFile(string senderId, string senderName, string fileData, string fileName, string? recipientId, bool isGroup)
    {
        var fileUrl = await _fileUploadService.SaveFileAsync(fileData, fileName, MessageType.File);
        var chatMessage = new ChatMessage
        {
            User = senderName,
            SenderId = senderId,
            RecipientId = recipientId,
            IsGroup = isGroup,
            Content = fileUrl,
            FileUrl = fileUrl,
            Type = MessageType.File,
            FileName = fileName,
            Timestamp = DateTime.UtcNow
        };

        await _chatService.AddMessageAsync(chatMessage);
        if (isGroup)
        {
            await SendGroupMessage(chatMessage, senderId, recipientId);
            return;
        }

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return;
        }

        var recipient = _chatService.GetUserSessionById(recipientId);
        if (recipient != null)
        {
            await Clients.Clients(Context.ConnectionId, recipient.ConnectionId)
                .SendAsync("ReceiveMessage", chatMessage);
        }
    }

    public async Task UserTyping(string senderId, string senderName, string? recipientId, bool isGroup)
    {
        if (isGroup)
        {
            await Clients.Others.SendAsync("UserTyping", senderId, senderName, recipientId, isGroup);
            return;
        }

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return;
        }

        var recipient = _chatService.GetUserSessionById(recipientId);
        if (recipient != null)
        {
            await Clients.Client(recipient.ConnectionId)
                .SendAsync("UserTyping", senderId, senderName, recipientId, isGroup);
        }
    }

    public async Task MessageRead(string messageId)
    {
        await _chatService.MarkMessageAsReadAsync(messageId);
        await Clients.All.SendAsync("MessageRead", messageId);
    }

    public async Task ReplyMessage(string senderId, string senderName, string message, string? recipientId, bool isGroup, string replyToMessageId)
    {
        var chatMessage = new ChatMessage
        {
            User = senderName,
            SenderId = senderId,
            RecipientId = recipientId,
            IsGroup = isGroup,
            Content = message,
            Type = MessageType.Text,
            ReplyToMessageId = replyToMessageId,
            Timestamp = DateTime.UtcNow
        };

        await _chatService.AddMessageAsync(chatMessage);
        if (isGroup)
        {
            await SendGroupMessage(chatMessage, senderId, recipientId);
            return;
        }

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return;
        }

        var recipient = _chatService.GetUserSessionById(recipientId);
        if (recipient != null)
        {
            await Clients.Clients(Context.ConnectionId, recipient.ConnectionId)
                .SendAsync("ReceiveMessage", chatMessage);
        }
    }

    public async Task ForwardMessage(string senderId, string senderName, string messageId, string? recipientId, bool isGroup)
    {
        var originalMessage = await _chatService.GetMessageByIdAsync(messageId);
        if (originalMessage == null)
        {
            return;
        }

        var chatMessage = new ChatMessage
        {
            User = senderName,
            SenderId = senderId,
            RecipientId = recipientId,
            IsGroup = isGroup,
            Content = originalMessage.Content,
            Type = originalMessage.Type,
            FileName = originalMessage.FileName,
            Duration = originalMessage.Duration,
            FileUrl = originalMessage.FileUrl,
            ForwardedFromMessageId = messageId,
            Timestamp = DateTime.UtcNow
        };

        await _chatService.AddMessageAsync(chatMessage);
        if (isGroup)
        {
            await SendGroupMessage(chatMessage, senderId, recipientId);
            return;
        }

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            return;
        }

        var recipient = _chatService.GetUserSessionById(recipientId);
        if (recipient != null)
        {
            await Clients.Clients(Context.ConnectionId, recipient.ConnectionId)
                .SendAsync("ReceiveMessage", chatMessage);
        }
    }

    public async Task DeleteMessage(string messageId, string senderId)
    {
        var message = await _chatService.GetMessageByIdAsync(messageId);
        if (message != null && message.SenderId == senderId)
        {
            await _chatService.DeleteMessageAsync(messageId);
            await Clients.All.SendAsync("MessageDeleted", messageId);
        }
    }

    public async Task ClearChat(string userId, string? otherUserId, bool isGroup)
    {
        await _chatService.DeleteChatAsync(userId, otherUserId, isGroup);
        await Clients.All.SendAsync("ChatCleared", userId, otherUserId, isGroup);
    }

    private async Task SendUpdatedUserLists()
    {
        var allUsers = await _chatService.GetAllUsersAsync();
        var onlineUsers = _chatService.GetOnlineUsers();
        
        foreach (var onlineUser in onlineUsers)
        {
            // Send all users except the current user
            var usersToSend = allUsers.Where(u => u.Id != onlineUser.Id).ToList();
            await Clients.Client(onlineUser.ConnectionId).SendAsync("UpdateUserList", usersToSend);
        }
    }

    private async Task<bool> SendGroupMessage(ChatMessage chatMessage, string senderId, string? recipientId)
    {
        // If recipientId is provided for a group message, treat it as a private group (GroupId)
        if (!string.IsNullOrWhiteSpace(recipientId))
        {
            // Private group message - validate membership and send only to members
            var groupId = recipientId;
            chatMessage.GroupId = groupId;
            
            // Validate sender is a member
            if (!await _groupService.IsUserMemberAsync(groupId, senderId))
            {
                return false; // Sender is not a member, reject message
            }
            
            // Get all group members
            var members = await _groupService.GetGroupMembersAsync(groupId);
            var connectionIds = new List<string>();
            
            foreach (var member in members)
            {
                var sessionUser = _chatService.GetUserSessionById(member.Id);
                if (sessionUser != null)
                {
                    connectionIds.Add(sessionUser.ConnectionId);
                }
            }
            
            // Send to all online group members
            if (connectionIds.Any())
            {
                await Clients.Clients(connectionIds).SendAsync("ReceiveMessage", chatMessage);
            }
        }
        else
        {
            // Global group chat - send to all
            await Clients.All.SendAsync("ReceiveMessage", chatMessage);
        }
        return true;
    }
}
