<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html>
<html>
<head>
    <title>ASP.NET WebForm 4.8 - Chat Integration Example</title>
    <script src="https://your-blazor-chat-url/Integration/webform-integration.js"></script>
    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 20px;
        }
        
        .container {
            max-width: 800px;
            margin: 0 auto;
        }
        
        .demo-section {
            margin: 30px 0;
            padding: 20px;
            border: 1px solid #ddd;
            border-radius: 5px;
        }
        
        .demo-section h2 {
            margin-top: 0;
            color: #075E54;
        }
        
        button {
            background-color: #075E54;
            color: white;
            padding: 10px 20px;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            margin: 5px;
        }
        
        button:hover {
            background-color: #128C7E;
        }
        
        #chatContainer {
            height: 600px;
            border: 1px solid #ddd;
            margin-top: 10px;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>ASP.NET WebForm 4.8 - Blazor Chat Integration Examples</h1>
        <p>This page demonstrates different ways to integrate the Blazor Chat application into your ASP.NET WebForm 4.8 application.</p>

        <!-- Example 1: Popup Window -->
        <div class="demo-section">
            <h2>Method 1: Open Chat in Popup Window</h2>
            <p>Opens the chat in a new popup window, perfect for a standalone chat experience.</p>
            <button onclick="openChatPopupExample()">Open Chat Popup</button>
        </div>

        <!-- Example 2: Floating Button -->
        <div class="demo-section">
            <h2>Method 2: Floating Chat Button</h2>
            <p>Adds a floating chat button that stays on the page. Click it to open the chat.</p>
            <button onclick="addFloatingButtonExample()">Add Floating Chat Button</button>
        </div>

        <!-- Example 3: Embedded -->
        <div class="demo-section">
            <h2>Method 3: Embed Chat in Page</h2>
            <p>Embeds the chat directly into the page using an iframe.</p>
            <button onclick="embedChatExample()">Embed Chat</button>
            <div id="chatContainer"></div>
        </div>
    </div>

    <script runat="server">
        // Code-behind methods to get user information
        protected string GetCurrentUserId()
        {
            // In a real application, get this from Session or authentication
            return Session["UserId"]?.ToString() ?? "123";
        }

        protected string GetCurrentUserName()
        {
            // In a real application, get this from Session or authentication
            return Session["UserName"]?.ToString() ?? "Demo User";
        }
    </script>

    <script type="text/javascript">
        // Configuration
        const BLAZOR_CHAT_URL = 'https://localhost:5001'; // Change this to your Blazor app URL
        
        // Get user info from server
        // In a real application, you would get these from session or authentication
        const userId = '<%= GetCurrentUserId() %>';
        const userName = '<%= GetCurrentUserName() %>';

        // Example 1: Open in popup
        function openChatPopupExample() {
            openChatPopup(BLAZOR_CHAT_URL, userId, userName, {
                width: 400,
                height: 600,
                left: screen.width - 420,
                top: 100
            });
        }

        // Example 2: Floating button
        let floatingButton = null;
        function addFloatingButtonExample() {
            if (floatingButton) {
                alert('Floating button already added! Check the bottom-right corner.');
                return;
            }
            
            floatingButton = createFloatingChatButton(BLAZOR_CHAT_URL, userId, userName, {
                position: 'bottom-right',
                backgroundColor: '#075E54',
                text: '💬'
            });
        }

        // Example 3: Embed in page
        function embedChatExample() {
            embedChatIframe('chatContainer', BLAZOR_CHAT_URL, userId, userName, {
                width: '100%',
                height: '600px'
            });
        }

        // Optional: Add floating button on page load
        // Uncomment the following line to automatically add a floating chat button
        // window.onload = addFloatingButtonExample;
    </script>
</body>
</html>
