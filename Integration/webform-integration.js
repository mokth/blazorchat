// JavaScript for integrating Blazor Chat with ASP.NET WebForms 4.8

/**
 * Opens the Blazor chat application in a popup window
 * @param {string} userId - The user's unique identifier
 * @param {string} userName - The user's display name
 * @param {string} blazorAppUrl - The URL of your Blazor chat application (optional, defaults to current origin)
 */
function openChatPopup(userId, userName, blazorAppUrl) {
    // Use provided URL or default to current origin
    const appUrl = blazorAppUrl || window.location.origin;
    
    // Construct the chat URL with parameters
    const chatUrl = `${appUrl}/chat?userId=${encodeURIComponent(userId)}&userName=${encodeURIComponent(userName)}`;
    
    // Open popup window
    const popup = window.open(
        chatUrl,
        'BlazorChatPopup',
        'width=450,height=650,resizable=yes,scrollbars=yes,status=no,location=no,toolbar=no,menubar=no'
    );
    
    if (popup) {
        popup.focus();
    } else {
        alert('Popup was blocked. Please allow popups for this site.');
    }
    
    return popup;
}

/**
 * Opens the chat in an iframe within the current page
 * @param {string} userId - The user's unique identifier
 * @param {string} userName - The user's display name
 * @param {string} containerId - The ID of the container element for the iframe
 * @param {string} blazorAppUrl - The URL of your Blazor chat application (optional)
 */
function openChatIframe(userId, userName, containerId, blazorAppUrl) {
    const appUrl = blazorAppUrl || window.location.origin;
    const chatUrl = `${appUrl}/chat?userId=${encodeURIComponent(userId)}&userName=${encodeURIComponent(userName)}`;
    
    const container = document.getElementById(containerId);
    if (!container) {
        console.error(`Container with ID '${containerId}' not found`);
        return;
    }
    
    // Create iframe
    const iframe = document.createElement('iframe');
    iframe.src = chatUrl;
    iframe.style.width = '100%';
    iframe.style.height = '100%';
    iframe.style.border = 'none';
    iframe.title = 'Blazor Chat';
    
    // Clear container and add iframe
    container.innerHTML = '';
    container.appendChild(iframe);
    
    return iframe;
}

/**
 * Creates a floating chat button that opens the chat in a popup
 * @param {string} userId - The user's unique identifier
 * @param {string} userName - The user's display name
 * @param {string} blazorAppUrl - The URL of your Blazor chat application (optional)
 */
function createChatButton(userId, userName, blazorAppUrl) {
    // Create button
    const button = document.createElement('button');
    button.id = 'blazor-chat-button';
    button.innerHTML = '💬';
    button.title = 'Open Chat';
    
    // Style button
    button.style.position = 'fixed';
    button.style.bottom = '20px';
    button.style.right = '20px';
    button.style.width = '60px';
    button.style.height = '60px';
    button.style.borderRadius = '50%';
    button.style.backgroundColor = '#075E54';
    button.style.color = 'white';
    button.style.border = 'none';
    button.style.fontSize = '30px';
    button.style.cursor = 'pointer';
    button.style.boxShadow = '0 4px 12px rgba(0,0,0,0.3)';
    button.style.zIndex = '9999';
    button.style.transition = 'all 0.3s ease';
    
    // Add hover effect
    button.onmouseover = function() {
        this.style.backgroundColor = '#0A7C68';
        this.style.transform = 'scale(1.1)';
    };
    button.onmouseout = function() {
        this.style.backgroundColor = '#075E54';
        this.style.transform = 'scale(1)';
    };
    
    // Add click handler
    button.onclick = function() {
        openChatPopup(userId, userName, blazorAppUrl);
    };
    
    // Add to page
    document.body.appendChild(button);
    
    return button;
}

/**
 * Example usage in ASP.NET WebForm:
 * 
 * <!-- Add this in your WebForm page -->
 * <script src="webform-integration.js"></script>
 * <script>
 *     // Get user info from server-side (example)
 *     var userId = '<%= Session["UserId"] %>';
 *     var userName = '<%= Session["UserName"] %>';
 *     
 *     // Option 1: Create a floating chat button
 *     createChatButton(userId, userName, 'https://your-blazor-app-url.com');
 *     
 *     // Option 2: Open chat on button click
 *     document.getElementById('yourButtonId').onclick = function() {
 *         openChatPopup(userId, userName, 'https://your-blazor-app-url.com');
 *     };
 *     
 *     // Option 3: Embed chat in an iframe
 *     openChatIframe(userId, userName, 'chatContainer', 'https://your-blazor-app-url.com');
 * </script>
 */
