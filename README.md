# Blazor Chat Application

A real-time chat application built with **Blazor .NET 8** and **SignalR** featuring a modern WhatsApp-style UI with support for text messages, voice messages, images, and file attachments.

## Features

- ✅ **Real-time messaging** with SignalR
- ✅ **Modern WhatsApp-style UI** with message bubbles
- ✅ **Text messages** - Instant messaging
- ✅ **Voice messages** - Record and send audio
- ✅ **Image sharing** - Upload and preview images
- ✅ **File attachments** - Share documents and files
- ✅ **User presence** - Online/offline status
- ✅ **Typing indicators** - See when others are typing
- ✅ **Read receipts** - Message delivery confirmation
- ✅ **Responsive design** - Works on desktop and mobile
- ✅ **WebForm integration** - Can be opened as popup from ASP.NET WebForm 4.8

## Technology Stack

- **Blazor Server .NET 8**
- **SignalR** for real-time communication
- **ASP.NET Core 8.0**
- **JavaScript** for voice recording (MediaRecorder API)
- **Modern CSS** with WhatsApp-inspired design

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- A modern web browser (Chrome, Firefox, Edge, Safari)
- Microphone (for voice messages)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/mokth/blazorchat.git
cd blazorchat
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Run the application

```bash
dotnet run
```

The application will start at `https://localhost:5001` (or the port shown in the console).

### 4. Open in browser

Navigate to `https://localhost:5001` or `https://localhost:5001/chat`

## Usage

### Basic Chat

1. Open the application in your browser
2. Enter your name on the login screen
3. Click "Join Chat"
4. Start sending messages!

### Sending Messages

- **Text**: Type in the input field and press Enter or click the send button
- **Images**: Click the camera icon (📷) to select and send images
- **Files**: Click the attachment icon (📎) to select and send files
- **Voice**: Click the microphone icon (🎤) to record, click again to stop and send

### Multiple Users

Open multiple browser windows or tabs to simulate multiple users chatting in real-time.

## Integration with ASP.NET WebForm 4.8

### Method 1: Popup Window

Add this to your WebForm page:

```html
<script src="https://your-blazor-url/Integration/webform-integration.js"></script>
<script>
    function openChat() {
        var userId = '<%= Session["UserId"] %>';
        var userName = '<%= Session["UserName"] %>';
        openChatPopup('https://your-blazor-url', userId, userName);
    }
</script>

<button onclick="openChat()">Open Chat</button>
```

### Method 2: Floating Chat Button

```html
<script src="https://your-blazor-url/Integration/webform-integration.js"></script>
<script>
    window.onload = function() {
        var userId = '<%= Session["UserId"] %>';
        var userName = '<%= Session["UserName"] %>';
        createFloatingChatButton('https://your-blazor-url', userId, userName);
    };
</script>
```

### Method 3: Embedded in Page

```html
<div id="chatContainer"></div>

<script src="https://your-blazor-url/Integration/webform-integration.js"></script>
<script>
    window.onload = function() {
        var userId = '<%= Session["UserId"] %>';
        var userName = '<%= Session["UserName"] %>';
        embedChatIframe('chatContainer', 'https://your-blazor-url', userId, userName, {
            width: '100%',
            height: '600px'
        });
    };
</script>
```

## Project Structure

```
blazorchat/
├── Program.cs                      # Application entry point and configuration
├── appsettings.json                # Configuration settings
├── blazorchat.csproj               # Project file
├── Hubs/
│   └── ChatHub.cs                  # SignalR hub for real-time messaging
├── Models/
│   ├── ChatMessage.cs              # Message model
│   ├── User.cs                     # User model
│   └── MessageType.cs              # Message type enum
├── Services/
│   ├── IChatService.cs             # Chat service interface
│   ├── ChatService.cs              # Chat business logic
│   └── FileUploadService.cs        # File upload handling
├── Components/
│   ├── App.razor                   # Root component
│   ├── Routes.razor                # Routing configuration
│   ├── _Imports.razor              # Global imports
│   ├── Layout/
│   │   ├── MainLayout.razor        # Main layout
│   │   └── MainLayout.razor.css
│   ├── Pages/
│   │   ├── Home.razor              # Home page
│   │   └── Chat.razor              # Chat page
│   └── Shared/
│       ├── ChatWindow.razor        # Chat window component
│       ├── ChatWindow.razor.css
│       ├── MessageBubble.razor     # Message bubble component
│       ├── MessageInput.razor      # Message input area
│       ├── UserList.razor          # User list sidebar
│       └── VoiceRecorder.razor     # Voice recording component
├── wwwroot/
│   ├── css/
│   │   ├── app.css                 # Global styles
│   │   └── chat-styles.css         # Chat-specific styles
│   ├── js/
│   │   ├── chat.js                 # Chat JavaScript functions
│   │   └── voiceRecorder.js        # Voice recording functionality
│   └── uploads/                    # Uploaded files directory
└── Integration/
    ├── ChatPopup.html              # Popup page for WebForm integration
    └── webform-integration.js      # JavaScript integration helpers
```

## Configuration

Edit `appsettings.json` to customize:

- **File upload limits**: Maximum file size
- **Allowed file extensions**: Restrict file types
- **SignalR settings**: Connection timeouts
- **CORS settings**: Allowed origins

## Browser Compatibility

- ✅ Chrome 60+
- ✅ Firefox 55+
- ✅ Edge 79+
- ✅ Safari 11+
- ✅ Opera 47+

**Note**: Voice recording requires HTTPS in production and microphone permissions.

## Security Features

- File upload validation (size and type)
- Input sanitization
- CORS configuration
- Secure file storage

## Development

### Build

```bash
dotnet build
```

### Run in development mode

```bash
dotnet run --environment Development
```

### Publish for production

```bash
dotnet publish -c Release -o ./publish
```

## Troubleshooting

### Voice recording not working

- Ensure you're using HTTPS (required for microphone access)
- Grant microphone permissions in your browser
- Check browser console for errors

### Files not uploading

- Check file size limits in `appsettings.json`
- Verify file extension is allowed
- Ensure `wwwroot/uploads` directory exists and is writable

### SignalR connection issues

- Check firewall settings
- Verify WebSocket support is enabled
- Check browser console for connection errors

## License

This project is open source and available for educational and commercial use.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## Author

Built with ❤️ using Blazor .NET 8 and SignalR