# Blazor Chat Application

A real-time chat application built with **Blazor Server .NET 8** and **SignalR** featuring a modern WhatsApp-style UI with support for text messages, voice messages, images, and file attachments.

## Features

- ✅ **Real-time messaging** with SignalR
- ✅ **Modern WhatsApp-style UI** with green message bubbles
- ✅ **Voice message support** using MediaRecorder API
- ✅ **Image and file attachments** with upload preview
- ✅ **User presence detection** (online/offline status)
- ✅ **Typing indicators** showing when users are typing
- ✅ **User avatars** with initials
- ✅ **Message timestamps** in HH:mm format
- ✅ **Auto-scroll** to latest messages
- ✅ **Responsive design** for different screen sizes
- ✅ **Integration with ASP.NET WebForm 4.8** via popup/iframe

## Technical Stack

- **Blazor Server .NET 8** (NOT WebAssembly)
- **SignalR** for real-time communication
- **ASP.NET Core 8.0** hosting
- **JavaScript Interop** for file uploads and voice recording
- **CORS configured** for cross-origin integration

## Project Structure

```
blazorchat/
├── Program.cs                           # Blazor Server .NET 8 setup with SignalR
├── appsettings.json                     # Configuration (file upload limits, CORS)
├── blazorchat.csproj                    # .NET 8 project file
├── Models/
│   ├── ChatMessage.cs                   # Message model (text, image, voice, file)
│   ├── User.cs                          # User model with online status
│   ├── MessageType.cs                   # Enum for message types
│   └── VoiceRecordingResult.cs          # Voice recording result model
├── Services/
│   ├── IChatService.cs                  # Chat service interface
│   ├── ChatService.cs                   # Chat business logic (in-memory storage)
│   └── FileUploadService.cs             # Handle file/image/voice uploads
├── Hubs/
│   └── ChatHub.cs                       # SignalR hub for real-time messaging
├── Components/
│   ├── _Imports.razor                   # Global using directives
│   ├── App.razor                        # Root component
│   ├── Routes.razor                     # Routing configuration
│   ├── Pages/
│   │   └── Chat.razor                   # Main chat page with SignalR connection
│   ├── Layout/
│   │   ├── MainLayout.razor             # Layout for chat
│   │   └── MainLayout.razor.css         # Layout styles
│   └── Shared/
│       ├── ChatWindow.razor             # Chat window component
│       ├── ChatWindow.razor.css         # WhatsApp-style CSS
│       ├── MessageBubble.razor          # Individual message component
│       ├── MessageInput.razor           # Message input area with attachments
│       ├── UserList.razor               # User list sidebar
│       └── VoiceRecorder.razor          # Voice recording component
├── wwwroot/
│   ├── css/
│   │   ├── app.css                      # Global styles
│   │   └── chat-styles.css              # Chat-specific WhatsApp styles
│   ├── js/
│   │   ├── chat.js                      # JavaScript for chat features
│   │   └── voiceRecorder.js             # Voice recording functionality
│   └── uploads/                         # Folder for uploaded files (gitignored)
└── Integration/
    ├── ChatPopup.html                   # Standalone popup page
    └── webform-integration.js           # JavaScript for WebForm integration
```

## Setup & Installation

### Prerequisites
- .NET 8.0 SDK or later
- Modern web browser with WebRTC support (for voice messages)

### Running the Application

1. **Clone the repository:**
   ```bash
   git clone https://github.com/mokth/blazorchat.git
   cd blazorchat
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

4. **Open in browser:**
   Navigate to `http://localhost:5000` or `https://localhost:5001`

## Usage

### Basic Chat Usage

1. Enter your name on the login page
2. Click "Join Chat" to enter the chat room
3. Type your message in the input field at the bottom
4. Press Enter or click the send button (📤) to send

### Sending Images
- Click the image icon (🖼️) to select and upload an image
- Images are displayed inline in the chat with preview

### Sending Voice Messages
- Click the microphone icon (🎤) to start recording
- Click the stop icon (⏹️) to finish recording
- Voice messages appear with an audio player

### Sending Files
- Click the attachment icon (📎) to select and upload a file
- Files appear as downloadable attachments

## ASP.NET WebForm 4.8 Integration

### Option 1: Popup Window

Add this to your WebForm page:

```html
<script src="https://your-blazor-app-url/Integration/webform-integration.js"></script>
<script>
    // Get user info from server-side
    var userId = '<%= Session["UserId"] %>';
    var userName = '<%= Session["UserName"] %>';
    
    // Create a floating chat button
    createChatButton(userId, userName, 'https://your-blazor-app-url');
</script>
```

### Option 2: Inline Button

```html
<button onclick="openChatPopup('user123', 'John Doe', 'https://your-blazor-app-url')">
    Open Chat
</button>
```

### Option 3: Embedded iFrame

```html
<div id="chatContainer" style="width: 100%; height: 600px;"></div>
<script>
    openChatIframe('user123', 'John Doe', 'chatContainer', 'https://your-blazor-app-url');
</script>
```

## Configuration

### File Upload Settings (appsettings.json)

```json
{
  "FileUpload": {
    "MaxFileSizeMB": 10,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt", ".webm", ".mp3", ".wav", ".ogg"]
  }
}
```

### CORS Configuration

For production, update `Program.cs` to include your WebForm application URL:

```csharp
policy.SetIsOriginAllowed(origin =>
{
    return origin == "https://your-webform-app.com";
})
```

## Features in Detail

### SignalR Hub Methods

The `ChatHub` supports:
- `SendMessage(userId, userName, message)` - Send text message
- `SendImage(userId, userName, imageData, fileName)` - Send image
- `SendVoiceMessage(userId, userName, audioData, duration)` - Send voice
- `SendFile(userId, userName, fileData, fileName)` - Send attachment
- `UserTyping(userName)` - Typing indicator
- `UserConnected(userId, userName)` - User online status
- `OnDisconnectedAsync()` - User offline status

### WhatsApp-Style Design

- **Sent messages**: Light green (#DCF8C6) bubbles on the right
- **Received messages**: White (#FFFFFF) bubbles on the left
- **Background**: Subtle beige (#E5DDD5)
- **Header**: Teal green (#075E54)
- **Smooth animations** for new messages
- **Rounded corners** and shadows for depth

## Security Features

- ✅ File upload validation (size and extension checks)
- ✅ CORS configured with origin validation
- ✅ Input sanitization for user messages
- ✅ Secure file storage with unique filenames
- ✅ CodeQL security scanning passed

## Browser Compatibility

- **Chrome/Edge**: Full support (recommended)
- **Firefox**: Full support
- **Safari**: Full support (iOS 14.3+)
- **Voice recording** requires WebRTC support

## Known Limitations

- Messages are stored in-memory (not persisted to database)
- Voice messages work best in Chrome/Edge
- File uploads limited to 10MB by default
- No message history for new users joining after messages were sent

## Future Enhancements

- [ ] Database persistence for messages
- [ ] User authentication
- [ ] Private messaging between users
- [ ] Message editing and deletion
- [ ] Emoji picker
- [ ] Message search
- [ ] Push notifications
- [ ] Message read receipts
- [ ] File drag-and-drop support

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is open source and available under the MIT License.

## Screenshots

### Login Page
![Login Page](https://github.com/user-attachments/assets/ed41b9db-ab63-4157-9efc-44a6335e8f13)

### Chat Interface
![Chat Interface](https://github.com/user-attachments/assets/ad370036-d069-4130-8eff-198b6729d195)

### WhatsApp-style Message Bubbles
![Chat with Message](https://github.com/user-attachments/assets/0eeff721-7c8e-4001-a43d-3aa27fb26024)

## Support

For issues and questions, please open an issue on GitHub.