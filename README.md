# Blazor Chat Application

A complete real-time chat application built with **Blazor Server .NET 8** and **SignalR** featuring a modern WhatsApp-style UI with support for text messages, voice messages, images, and file attachments. Now with **database-backed user persistence** for reliable identity management.

## Features

### Core Chat Features
- ✅ **Real-time messaging** using SignalR
- ✅ **Database-persisted users** - accounts survive browser sessions
- ✅ **Text messages** with instant delivery
- ✅ **Voice messages** with recording and playback
- ✅ **Image sharing** with preview
- ✅ **File attachments** with download support
- ✅ **Typing indicators** to show when users are typing
- ✅ **Online/offline status** for all users
- ✅ **Message timestamps** with read receipts
- ✅ **User presence detection**
- ✅ **7-day message retention** with automatic cleanup
- ✅ **Message history** - conversations persist across sessions

### Advanced Features
- ✅ **Reply to messages** - quote and respond to specific messages
- ✅ **Forward messages** - share messages to other conversations
- ✅ **Delete messages** - remove your sent messages
- ✅ **Clear chat** - delete entire conversation history
- ✅ **Unread message counts** - track unread messages per conversation
- ✅ **Read receipts** - see when messages are read

### UI/UX Features
- ✅ **WhatsApp-style interface** with modern design
- ✅ **Message bubbles** with different colors for sent/received
- ✅ **User avatars** with initials
- ✅ **Message grouping** for consecutive messages
- ✅ **Auto-scroll** to latest messages
- ✅ **Responsive design** for mobile and desktop
- ✅ **Smooth animations** for new messages

### Integration
- ✅ **ASP.NET WebForm 4.8 integration** via popup or modal
- ✅ **URL parameter support** for passing user information
- ✅ **CORS configuration** for cross-origin requests

## Technology Stack

- **Blazor Server .NET 8** - Server-side rendering and interactivity
- **SignalR** - Real-time WebSocket communication
- **Entity Framework Core 8** - Database access and migrations
- **SQL Server** - User and message persistence
- **ASP.NET Core 8.0** - Modern web framework
- **MediaRecorder API** - Browser-based voice recording
- **CSS3** - Modern styling with animations

## Database Schema

### Users Table
- `Id` (string, PK) - Unique GUID identifier
- `Name` (string, unique) - Username
- `CreatedAt` (DateTime) - Account creation timestamp
- `LastSeen` (DateTime) - Last activity timestamp
- `AvatarUrl` (string, nullable) - User avatar URL

### ChatMessages Table
- `Id` (string, PK) - Message identifier
- `SenderId` (string, FK) - References Users.Id
- `RecipientId` (string, nullable, FK) - For direct messages
- `IsGroup` (bool) - Group chat flag
- `Content` (string) - Message text or file URL
- `Type` (enum) - Text, Image, Voice, File
- `Timestamp` (DateTime) - Message creation time
- `IsRead` (bool) - Read status
- `ReplyToMessageId` (string, nullable) - Reply reference
- `ForwardedFromMessageId` (string, nullable) - Forward reference

## Project Structure

```
blazorchat/
├── Program.cs                           # Blazor Server configuration
├── appsettings.json                     # Application settings
├── blazorchat.csproj                    # .NET 8 project file
├── TESTING.md                           # Test scenarios and procedures
├── ARCHITECTURE_CHANGES.md              # Detailed architecture documentation
├── Data/
│   └── ChatDbContext.cs                 # EF Core database context
├── Migrations/                          # EF Core migrations
│   ├── 20260207105902_AddReplyForwardDeleteFeatures.cs
│   └── 20260207152437_AddUsersTable.cs
├── Hubs/
│   └── ChatHub.cs                       # SignalR hub
├── Models/
│   ├── ChatMessage.cs                   # Message model
│   ├── User.cs                          # User model (EF Core entity)
│   └── MessageType.cs                   # Message type enum
├── Services/
│   ├── IChatService.cs                  # Chat service interface
│   ├── ChatService.cs                   # Chat business logic
│   ├── FileUploadService.cs             # File upload handling
│   └── ChatCleanupService.cs            # Background cleanup service
├── Components/
│   ├── App.razor                        # Root component
│   ├── Routes.razor                     # Routing configuration
│   ├── Pages/
│   │   ├── Chat.razor                   # Main chat page
│   │   └── Chat.razor.cs                # Chat page logic
│   ├── Layout/
│   │   ├── MainLayout.razor             # Layout component
│   │   └── MainLayout.razor.css
│   └── Shared/
│       ├── ChatWindow.razor             # Chat window component
│       ├── MessageBubble.razor          # Message bubble component
│       ├── MessageInput.razor           # Message input area
│       ├── UserList.razor               # User list sidebar
│       └── VoiceRecorder.razor          # Voice recording component
├── wwwroot/
│   ├── css/
│   │   ├── app.css                      # Global styles
│   │   └── chat-styles.css              # WhatsApp-style CSS
│   ├── js/
│   │   ├── chat.js                      # Chat JavaScript (no localStorage)
│   │   └── voiceRecorder.js             # Voice recording JS
│   └── uploads/                         # Uploaded files
└── Integration/
    ├── ChatPopup.html                   # Popup wrapper
    ├── webform-integration.js           # WebForm integration
    └── WebFormExample.html              # Integration example
```

## Getting Started

### Prerequisites
- .NET 8 SDK or later
- SQL Server or SQL Server Express
- Modern web browser (Chrome, Firefox, Edge, Safari)

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/mokth/blazorchat.git
   cd blazorchat
   ```

2. **Configure database connection:**
   
   Edit `appsettings.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "ChatDatabase": "Server=YOUR_SERVER;Database=BlazorChat;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

4. **Apply database migrations:**
   ```bash
   dotnet ef database update
   ```
   
   This creates the database and required tables (Users and ChatMessages).

5. **Run the application:**
   ```bash
   dotnet run
   ```

6. **Open in browser:**
   Navigate to `https://localhost:5001/chat` or `http://localhost:5000/chat`

## Usage

### Basic Chat Usage

1. Enter your name on the welcome screen
2. Click "Join Chat" to enter the chat room
3. Your account is automatically created or looked up in the database
4. Select a user from the list or click "Group Chat"
5. Type messages in the input box and press Enter or click Send
6. Use the toolbar buttons to:
   - 📷 Upload and send images
   - 🎤 Record and send voice messages
   - 📎 Attach and send files

### User Persistence

- **First time**: Enter a username → new account created in database
- **Returning**: Enter same username → same user ID retrieved, conversation history loads
- **Multiple sessions**: Same username across devices = same account
- **No localStorage**: All user data managed server-side in database

### Voice Messages

1. Click the microphone button (🎤) to start recording
2. Speak your message
3. Click the stop button (⏹️) to finish recording
4. The voice message will be automatically sent

### Image and File Sharing

1. Click the camera button (📷) for images or paperclip (📎) for files
2. Select a file from your device
3. The file will be uploaded and sent automatically
4. Recipients can view images or download files

### Message Actions

- **Reply**: Click the reply button on any message to quote and respond
- **Forward**: Click forward to share message to another conversation
- **Delete**: Click delete to remove your sent messages
- **Clear Chat**: Use the menu to clear entire conversation

## WebForm 4.8 Integration

### Method 1: Direct Popup

```html
<script src="Integration/webform-integration.js"></script>
<script>
  BlazorChatIntegration.configure({
    chatUrl: 'https://your-blazor-app-url.com'
  });

  function openChat() {
    var userName = '<%= Session["UserName"] %>';
    BlazorChatIntegration.openChatPopup(null, userName); // userId auto-assigned by server
  }
</script>
<asp:Button ID="btnOpenChat" runat="server" 
            Text="Open Chat" 
            OnClientClick="openChat(); return false;" />
```

### Method 2: Modal Iframe

```javascript
BlazorChatIntegration.openChatModal(null, userName);
```

### Method 3: Popup with Wrapper

```javascript
BlazorChatIntegration.openChatPopupWithWrapper(null, userName);
```

See `Integration/WebFormExample.html` for a complete example.

## Configuration

Edit `appsettings.json` to configure:

```json
{
  "ConnectionStrings": {
    "ChatDatabase": "Server=...;Database=BlazorChat;..."
  },
  "FileUpload": {
    "MaxImageSizeMB": 5,
    "MaxAudioSizeMB": 10,
    "MaxFileSizeMB": 20,
    "AllowedImageExtensions": [".jpg", ".jpeg", ".png", ".gif", ".webp"],
    "AllowedAudioExtensions": [".mp3", ".wav", ".webm", ".ogg"],
    "AllowedFileExtensions": [".pdf", ".doc", ".docx", ".txt", ".zip"],
    "UploadPath": "wwwroot/uploads"
  },
  "ChatRetention": {
    "HistoryDays": 7,
    "CleanupAfterDays": 20,
    "CleanupIntervalMinutes": 60
  },
  "SignalR": {
    "KeepAliveInterval": "00:00:15",
    "ClientTimeoutInterval": "00:00:30"
  },
  "CORS": {
    "AllowedOrigins": ["*"]
  }
}
```

## Architecture

### User Persistence Flow
1. User enters username → Client sends to server
2. Server queries database for existing user by name
3. If not found, creates new user record with GUID
4. Server sends userId back to client
5. User's message history loaded from database (last 7 days)
6. In-memory session tracks ConnectionId for real-time messaging

### Message Flow
1. Client sends message with userId
2. Server validates session and saves to database
3. Server routes message to recipient(s) via SignalR
4. Recipients receive real-time notification

See `ARCHITECTURE_CHANGES.md` for detailed architecture documentation.

## Testing

Comprehensive test scenarios available in `TESTING.md` including:
- User registration and login
- Message persistence and retrieval
- Multi-user interactions
- 7-day retention verification
- Database integrity checks

## Security

- ✅ No client-side storage of user identity
- ✅ Server-controlled user authentication
- ✅ File uploads validated for size and extension
- ✅ User inputs sanitized
- ✅ CORS configured for WebForm integration
- ✅ SignalR connections authenticated per session
- ✅ Database constraints prevent duplicate usernames
- ✅ SQL injection protection via Entity Framework

## Browser Support

- Chrome 60+
- Firefox 55+
- Safari 11+
- Edge 79+

**Note:** Voice recording requires browser support for MediaRecorder API and microphone permissions.

## Database Management

### Migrations

Create new migration:
```bash
dotnet ef migrations add MigrationName
```

Apply migrations:
```bash
dotnet ef database update
```

Remove last migration:
```bash
dotnet ef migrations remove
```

### Cleanup

The application automatically:
- Shows messages from last 7 days to users
- Deletes messages older than 20 days (runs every 60 minutes)
- Updates user LastSeen timestamp on activity

## Troubleshooting

### Voice recording not working
- Ensure microphone permissions are granted
- Use HTTPS in production (required for microphone access)
- Check browser console for errors

### Files not uploading
- Verify file size is under the limit (default 5-20MB depending on type)
- Check file extension is allowed in `appsettings.json`
- Ensure `wwwroot/uploads` directory exists and has write permissions

### WebForm integration not working
- Verify CORS settings in `appsettings.json`
- Check that the Blazor app URL is correct in integration script
- Enable popups in browser settings

### Database connection errors
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure database exists: `dotnet ef database update`
- Check firewall settings if using remote SQL Server

### User not persisting
- Verify database migrations are applied
- Check Users table exists in database
- Review server logs for SQL errors
- Ensure unique username constraint is not violated

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is open source and available under the MIT License.