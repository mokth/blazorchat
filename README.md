# Blazor Chat Application

A complete real-time chat application built with **Blazor Server .NET 8** and **SignalR** featuring a modern WhatsApp-style UI with support for text messages, voice messages, images, and file attachments.

## Features

### Core Chat Features
- ✅ **Real-time messaging** using SignalR
- ✅ **Text messages** with instant delivery
- ✅ **Voice messages** with recording and playback
- ✅ **Image sharing** with preview
- ✅ **File attachments** with download support
- ✅ **Typing indicators** to show when users are typing
- ✅ **Online/offline status** for all users
- ✅ **Message timestamps** with read receipts
- ✅ **User presence detection**

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
- **ASP.NET Core 8.0** - Modern web framework
- **MediaRecorder API** - Browser-based voice recording
- **CSS3** - Modern styling with animations

## Project Structure

```
blazorchat/
├── Program.cs                           # Blazor Server configuration
├── appsettings.json                     # Application settings
├── blazorchat.csproj                    # .NET 8 project file
├── Hubs/
│   └── ChatHub.cs                       # SignalR hub
├── Models/
│   ├── ChatMessage.cs                   # Message model
│   ├── User.cs                          # User model
│   └── MessageType.cs                   # Message type enum
├── Services/
│   ├── IChatService.cs                  # Chat service interface
│   ├── ChatService.cs                   # Chat business logic
│   └── FileUploadService.cs             # File upload handling
├── Components/
│   ├── App.razor                        # Root component
│   ├── Routes.razor                     # Routing configuration
│   ├── Pages/
│   │   └── Chat.razor                   # Main chat page
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
│   │   ├── chat.js                      # Chat JavaScript
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
- Modern web browser (Chrome, Firefox, Edge, Safari)

### Installation

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
   Navigate to `https://localhost:5001/chat` or `http://localhost:5000/chat`

## Usage

### Basic Chat Usage

1. Enter your name on the welcome screen
2. Click "Join Chat" to enter the chat room
3. Type messages in the input box and press Enter or click Send
4. Use the toolbar buttons to:
   - 📷 Upload and send images
   - 🎤 Record and send voice messages
   - 📎 Attach and send files

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

## WebForm 4.8 Integration

### Method 1: Direct Popup

```html
<script src="Integration/webform-integration.js"></script>
<script>
  BlazorChatIntegration.configure({
    chatUrl: 'https://your-blazor-app-url.com'
  });

  function openChat() {
    var userId = '<%= Session["UserId"] %>';
    var userName = '<%= Session["UserName"] %>';
    BlazorChatIntegration.openChatPopup(userId, userName);
  }
</script>
<asp:Button ID="btnOpenChat" runat="server" 
            Text="Open Chat" 
            OnClientClick="openChat(); return false;" />
```

### Method 2: Modal Iframe

```javascript
BlazorChatIntegration.openChatModal(userId, userName);
```

### Method 3: Popup with Wrapper

```javascript
BlazorChatIntegration.openChatPopupWithWrapper(userId, userName);
```

See `Integration/WebFormExample.html` for a complete example.

## Configuration

Edit `appsettings.json` to configure:

```json
{
  "FileUpload": {
    "MaxFileSizeMB": 10,
    "AllowedExtensions": [".jpg", ".png", ".pdf", ".doc", ".mp3", ".wav"]
  },
  "SignalR": {
    "KeepAliveInterval": "00:00:15",
    "ClientTimeoutInterval": "00:00:30"
  }
}
```

## Security

- File uploads are validated for size and extension
- User inputs are sanitized
- CORS is configured for WebForm integration
- SignalR connections are authenticated per user session

## Browser Support

- Chrome 60+
- Firefox 55+
- Safari 11+
- Edge 79+

**Note:** Voice recording requires browser support for MediaRecorder API and microphone permissions.

## Troubleshooting

### Voice recording not working
- Ensure microphone permissions are granted
- Use HTTPS in production (required for microphone access)
- Check browser console for errors

### Files not uploading
- Verify file size is under the limit (default 10MB)
- Check file extension is allowed in `appsettings.json`
- Ensure `wwwroot/uploads` directory exists and has write permissions

### WebForm integration not working
- Verify CORS settings in `appsettings.json`
- Check that the Blazor app URL is correct in integration script
- Enable popups in browser settings

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is open source and available under the MIT License.