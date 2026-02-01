<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html>
<html>
<head>
    <title>WebForm Chat Integration Example</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 20px;
        }
        .container {
            max-width: 800px;
            margin: 0 auto;
        }
        h1 {
            color: #075E54;
        }
        .demo-section {
            margin: 30px 0;
            padding: 20px;
            background: #f5f5f5;
            border-radius: 8px;
        }
        .demo-section h2 {
            color: #128C7E;
            margin-top: 0;
        }
        button {
            background: #25D366;
            color: white;
            border: none;
            padding: 12px 24px;
            border-radius: 5px;
            cursor: pointer;
            font-size: 16px;
            margin: 5px;
        }
        button:hover {
            background: #1FAD53;
        }
        #chatContainer {
            width: 100%;
            height: 600px;
            border: 1px solid #ddd;
            border-radius: 8px;
            margin-top: 10px;
        }
        .code-block {
            background: #2d2d2d;
            color: #f8f8f2;
            padding: 15px;
            border-radius: 5px;
            overflow-x: auto;
            font-family: 'Courier New', monospace;
            font-size: 14px;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>🗨️ Blazor Chat Integration Examples</h1>
        <p>This page demonstrates different ways to integrate the Blazor Chat application into an ASP.NET WebForm 4.8 application.</p>
        
        <!-- Example 1: Popup Window -->
        <div class="demo-section">
            <h2>Option 1: Chat Popup Window</h2>
            <p>Opens the chat in a new popup window. Best for non-intrusive chat access.</p>
            <button onclick="openChatPopup('user123', 'John Doe', blazorAppUrl)">
                Open Chat Popup
            </button>
            
            <h3>Code Example:</h3>
            <div class="code-block">
&lt;script src="https://your-blazor-app-url/Integration/webform-integration.js"&gt;&lt;/script&gt;
&lt;script&gt;
    var userId = '&lt;%= Session["UserId"] %&gt;';
    var userName = '&lt;%= Session["UserName"] %&gt;';
    var blazorAppUrl = 'https://your-blazor-app-url';
    
    function openChat() {
        openChatPopup(userId, userName, blazorAppUrl);
    }
&lt;/script&gt;
&lt;button onclick="openChat()"&gt;Open Chat&lt;/button&gt;
            </div>
        </div>
        
        <!-- Example 2: Floating Chat Button -->
        <div class="demo-section">
            <h2>Option 2: Floating Chat Button</h2>
            <p>Creates a floating chat button in the bottom-right corner of the page.</p>
            <button onclick="addFloatingButton()">
                Add Floating Chat Button
            </button>
            
            <h3>Code Example:</h3>
            <div class="code-block">
&lt;script src="https://your-blazor-app-url/Integration/webform-integration.js"&gt;&lt;/script&gt;
&lt;script&gt;
    window.onload = function() {
        var userId = '&lt;%= Session["UserId"] %&gt;';
        var userName = '&lt;%= Session["UserName"] %&gt;';
        createChatButton(userId, userName, 'https://your-blazor-app-url');
    };
&lt;/script&gt;
            </div>
        </div>
        
        <!-- Example 3: Embedded iFrame -->
        <div class="demo-section">
            <h2>Option 3: Embedded iFrame</h2>
            <p>Embeds the chat directly in the page. Best for dedicated chat pages.</p>
            <button onclick="loadChatIframe()">
                Load Chat in iFrame
            </button>
            <button onclick="clearIframe()">
                Clear iFrame
            </button>
            <div id="chatContainer"></div>
            
            <h3>Code Example:</h3>
            <div class="code-block">
&lt;script src="https://your-blazor-app-url/Integration/webform-integration.js"&gt;&lt;/script&gt;
&lt;div id="chatContainer" style="width: 100%; height: 600px;"&gt;&lt;/div&gt;
&lt;script&gt;
    var userId = '&lt;%= Session["UserId"] %&gt;';
    var userName = '&lt;%= Session["UserName"] %&gt;';
    openChatIframe(userId, userName, 'chatContainer', 'https://your-blazor-app-url');
&lt;/script&gt;
            </div>
        </div>
        
        <!-- Server-Side Integration -->
        <div class="demo-section">
            <h2>Server-Side Integration (Code-Behind)</h2>
            <p>Example of how to pass user information from WebForm code-behind to the chat.</p>
            
            <h3>Code-Behind (C#):</h3>
            <div class="code-block">
protected void Page_Load(object sender, EventArgs e)
{
    // Get user info from session or database
    string userId = Session["UserId"]?.ToString() ?? "guest";
    string userName = Session["UserName"]?.ToString() ?? "Guest User";
    
    // Register startup script to initialize chat
    string script = string.Format(@"
        window.onload = function() {{
            openChatPopup('{0}', '{1}', 'https://your-blazor-app-url');
        }};
    ", userId, userName);
    
    ClientScript.RegisterStartupScript(this.GetType(), "InitChat", script, true);
}
            </div>
        </div>
        
        <!-- Configuration Notes -->
        <div class="demo-section">
            <h2>⚙️ Configuration Notes</h2>
            <ul>
                <li><strong>CORS:</strong> Make sure the Blazor app's CORS policy includes your WebForm URL</li>
                <li><strong>User Authentication:</strong> Pass authenticated user information via URL parameters</li>
                <li><strong>SSL:</strong> Both applications should use HTTPS in production</li>
                <li><strong>Session Management:</strong> User IDs should be consistent across both applications</li>
                <li><strong>Popup Blockers:</strong> Inform users to allow popups for your domain</li>
            </ul>
        </div>
    </div>
    
    <!-- Load the integration script from your Blazor app -->
    <script>
        // Update this URL to match your Blazor application URL
        var blazorAppUrl = window.location.origin;
        
        // Load the integration script
        var script = document.createElement('script');
        script.src = blazorAppUrl + '/Integration/webform-integration.js';
        document.head.appendChild(script);
        
        // Demo functions for this example page
        function addFloatingButton() {
            createChatButton('demo-user', 'Demo User', blazorAppUrl);
        }
        
        function loadChatIframe() {
            openChatIframe('demo-user', 'Demo User', 'chatContainer', blazorAppUrl);
        }
        
        function clearIframe() {
            document.getElementById('chatContainer').innerHTML = '';
        }
    </script>
</body>
</html>
