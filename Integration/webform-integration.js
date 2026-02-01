// WebForm Integration JavaScript
// This file provides functions to integrate Blazor Chat into ASP.NET WebForm 4.8 applications

/**
 * Opens the chat application in a popup window
 * @param {string} blazorChatUrl - The URL of the Blazor chat application (e.g., 'https://localhost:5001')
 * @param {string} userId - The user ID from your WebForm application
 * @param {string} userName - The user name to display in chat
 * @param {object} options - Optional configuration for popup window
 */
function openChatPopup(blazorChatUrl, userId, userName, options) {
    const defaults = {
        width: 400,
        height: 600,
        resizable: 'yes',
        scrollbars: 'yes',
        left: screen.width - 420,
        top: 100
    };
    
    const config = { ...defaults, ...options };
    
    // Build URL with parameters
    const chatUrl = `${blazorChatUrl}/Integration/ChatPopup.html?userId=${encodeURIComponent(userId)}&userName=${encodeURIComponent(userName)}`;
    
    // Build window features string
    const features = `width=${config.width},height=${config.height},resizable=${config.resizable},scrollbars=${config.scrollbars},left=${config.left},top=${config.top}`;
    
    // Open popup window
    const popup = window.open(chatUrl, 'BlazorChatPopup', features);
    
    if (!popup) {
        alert('Pop-up blocked! Please allow pop-ups for this site.');
        return null;
    }
    
    return popup;
}

/**
 * Opens the chat application directly in the current page
 * @param {string} blazorChatUrl - The URL of the Blazor chat application
 * @param {string} userId - The user ID
 * @param {string} userName - The user name
 */
function openChatDirect(blazorChatUrl, userId, userName) {
    const chatUrl = `${blazorChatUrl}/chat?userId=${encodeURIComponent(userId)}&userName=${encodeURIComponent(userName)}`;
    window.location.href = chatUrl;
}

/**
 * Embeds the chat application in an iframe
 * @param {string} containerId - The ID of the container element
 * @param {string} blazorChatUrl - The URL of the Blazor chat application
 * @param {string} userId - The user ID
 * @param {string} userName - The user name
 * @param {object} options - Optional styling options
 */
function embedChatIframe(containerId, blazorChatUrl, userId, userName, options) {
    const defaults = {
        width: '100%',
        height: '600px',
        border: 'none'
    };
    
    const config = { ...defaults, ...options };
    
    const container = document.getElementById(containerId);
    if (!container) {
        console.error(`Container element with ID '${containerId}' not found`);
        return;
    }
    
    // Build chat URL
    const chatUrl = `${blazorChatUrl}/chat?userId=${encodeURIComponent(userId)}&userName=${encodeURIComponent(userName)}`;
    
    // Create iframe
    const iframe = document.createElement('iframe');
    iframe.src = chatUrl;
    iframe.style.width = config.width;
    iframe.style.height = config.height;
    iframe.style.border = config.border;
    iframe.frameBorder = '0';
    
    // Clear container and add iframe
    container.innerHTML = '';
    container.appendChild(iframe);
}

/**
 * Creates a floating chat button that opens the chat popup when clicked
 * @param {string} blazorChatUrl - The URL of the Blazor chat application
 * @param {string} userId - The user ID
 * @param {string} userName - The user name
 * @param {object} options - Optional styling options
 */
function createFloatingChatButton(blazorChatUrl, userId, userName, options) {
    const defaults = {
        position: 'bottom-right',
        backgroundColor: '#075E54',
        color: 'white',
        size: '60px',
        text: '💬'
    };
    
    const config = { ...defaults, ...options };
    
    // Create button
    const button = document.createElement('button');
    button.innerHTML = config.text;
    button.style.cssText = `
        position: fixed;
        ${config.position.includes('bottom') ? 'bottom: 20px;' : 'top: 20px;'}
        ${config.position.includes('right') ? 'right: 20px;' : 'left: 20px;'}
        width: ${config.size};
        height: ${config.size};
        border-radius: 50%;
        background-color: ${config.backgroundColor};
        color: ${config.color};
        border: none;
        font-size: 24px;
        cursor: pointer;
        box-shadow: 0 4px 12px rgba(0,0,0,0.3);
        z-index: 9999;
        transition: transform 0.2s, box-shadow 0.2s;
    `;
    
    button.onmouseover = () => {
        button.style.transform = 'scale(1.1)';
        button.style.boxShadow = '0 6px 16px rgba(0,0,0,0.4)';
    };
    
    button.onmouseout = () => {
        button.style.transform = 'scale(1)';
        button.style.boxShadow = '0 4px 12px rgba(0,0,0,0.3)';
    };
    
    button.onclick = () => {
        openChatPopup(blazorChatUrl, userId, userName);
    };
    
    // Add to page
    document.body.appendChild(button);
    
    return button;
}

// Example usage (can be called from WebForm code-behind or inline JavaScript)
/*
<script>
    // Example 1: Open as popup
    function openChat() {
        openChatPopup('https://localhost:5001', '123', 'John Doe');
    }
    
    // Example 2: Embed in page
    window.onload = function() {
        embedChatIframe('chatContainer', 'https://localhost:5001', '123', 'John Doe');
    };
    
    // Example 3: Floating button
    window.onload = function() {
        const userId = '<%= Session["UserId"] %>';
        const userName = '<%= Session["UserName"] %>';
        createFloatingChatButton('https://localhost:5001', userId, userName);
    };
</script>
*/
