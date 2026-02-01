/**
 * JavaScript for integrating BlazorChat with ASP.NET WebForm 4.8 applications
 * 
 * Usage in WebForm:
 * 1. Include this script in your WebForm page
 * 2. Call openChatPopup(userId, userName) to open the chat
 * 
 * Example:
 * <script src="webform-integration.js"></script>
 * <script>
 *   function openChat() {
 *     var userId = '<%= Session["UserId"] %>';
 *     var userName = '<%= Session["UserName"] %>';
 *     BlazorChatIntegration.openChatPopup(userId, userName);
 *   }
 * </script>
 * <button onclick="openChat()">Open Chat</button>
 */

var BlazorChatIntegration = (function() {
    // Configuration
    var config = {
        chatUrl: 'http://localhost:5000', // Change to your Blazor app URL
        popupWidth: 400,
        popupHeight: 600,
        popupTitle: 'Chat'
    };

    var chatPopupWindow = null;

    /**
     * Open chat in a popup window
     * @param {string} userId - User ID from WebForm session
     * @param {string} userName - User name from WebForm session
     */
    function openChatPopup(userId, userName) {
        // Close existing popup if open
        if (chatPopupWindow && !chatPopupWindow.closed) {
            chatPopupWindow.focus();
            return;
        }

        // Calculate popup position (centered on screen)
        var screenWidth = window.screen.width;
        var screenHeight = window.screen.height;
        var left = (screenWidth - config.popupWidth) / 2;
        var top = (screenHeight - config.popupHeight) / 2;

        var popupFeatures = 
            'width=' + config.popupWidth + ',' +
            'height=' + config.popupHeight + ',' +
            'left=' + left + ',' +
            'top=' + top + ',' +
            'resizable=yes,' +
            'scrollbars=yes,' +
            'status=yes';

        // Build chat URL with parameters
        var chatPageUrl = config.chatUrl + '/chat' +
            '?userId=' + encodeURIComponent(userId) +
            '&userName=' + encodeURIComponent(userName);

        // Open popup
        chatPopupWindow = window.open(chatPageUrl, config.popupTitle, popupFeatures);

        if (!chatPopupWindow) {
            alert('Please allow popups for this site to use the chat feature.');
        }

        return chatPopupWindow;
    }

    /**
     * Open chat using ChatPopup.html wrapper
     * @param {string} userId - User ID from WebForm session
     * @param {string} userName - User name from WebForm session
     */
    function openChatPopupWithWrapper(userId, userName) {
        // Close existing popup if open
        if (chatPopupWindow && !chatPopupWindow.closed) {
            chatPopupWindow.focus();
            return;
        }

        // Calculate popup position (centered on screen)
        var screenWidth = window.screen.width;
        var screenHeight = window.screen.height;
        var left = (screenWidth - config.popupWidth) / 2;
        var top = (screenHeight - config.popupHeight) / 2;

        var popupFeatures = 
            'width=' + config.popupWidth + ',' +
            'height=' + config.popupHeight + ',' +
            'left=' + left + ',' +
            'top=' + top + ',' +
            'resizable=yes,' +
            'scrollbars=yes,' +
            'status=yes';

        // Build URL to ChatPopup.html with parameters
        var popupUrl = config.chatUrl + '/Integration/ChatPopup.html' +
            '?chatUrl=' + encodeURIComponent(config.chatUrl) +
            '&userId=' + encodeURIComponent(userId) +
            '&userName=' + encodeURIComponent(userName);

        // Open popup
        chatPopupWindow = window.open(popupUrl, config.popupTitle, popupFeatures);

        if (!chatPopupWindow) {
            alert('Please allow popups for this site to use the chat feature.');
        }

        return chatPopupWindow;
    }

    /**
     * Open chat in an iframe modal within the WebForm page
     * @param {string} userId - User ID from WebForm session
     * @param {string} userName - User name from WebForm session
     */
    function openChatModal(userId, userName) {
        // Create modal overlay
        var overlay = document.createElement('div');
        overlay.id = 'chatModalOverlay';
        overlay.style.cssText = 
            'position: fixed; top: 0; left: 0; width: 100%; height: 100%; ' +
            'background: rgba(0,0,0,0.5); z-index: 9998; display: flex; ' +
            'justify-content: center; align-items: center;';

        // Create modal container
        var modal = document.createElement('div');
        modal.id = 'chatModal';
        modal.style.cssText = 
            'width: 90%; max-width: 500px; height: 90%; max-height: 700px; ' +
            'background: white; border-radius: 8px; overflow: hidden; ' +
            'box-shadow: 0 10px 40px rgba(0,0,0,0.3); z-index: 9999;';

        // Create close button
        var closeBtn = document.createElement('button');
        closeBtn.innerHTML = '&times;';
        closeBtn.style.cssText = 
            'position: absolute; top: 10px; right: 10px; z-index: 10000; ' +
            'background: #075E54; color: white; border: none; ' +
            'width: 30px; height: 30px; border-radius: 50%; cursor: pointer; ' +
            'font-size: 20px; line-height: 1;';
        closeBtn.onclick = closeChatModal;

        // Create iframe
        var iframe = document.createElement('iframe');
        iframe.style.cssText = 'width: 100%; height: 100%; border: none;';
        iframe.src = config.chatUrl + '/chat' +
            '?userId=' + encodeURIComponent(userId) +
            '&userName=' + encodeURIComponent(userName);

        // Assemble modal
        modal.appendChild(closeBtn);
        modal.appendChild(iframe);
        overlay.appendChild(modal);
        document.body.appendChild(overlay);

        // Close on overlay click
        overlay.onclick = function(e) {
            if (e.target === overlay) {
                closeChatModal();
            }
        };
    }

    /**
     * Close the chat modal
     */
    function closeChatModal() {
        var overlay = document.getElementById('chatModalOverlay');
        if (overlay) {
            overlay.remove();
        }
    }

    /**
     * Configure the integration
     * @param {object} options - Configuration options
     */
    function configure(options) {
        if (options.chatUrl) config.chatUrl = options.chatUrl;
        if (options.popupWidth) config.popupWidth = options.popupWidth;
        if (options.popupHeight) config.popupHeight = options.popupHeight;
        if (options.popupTitle) config.popupTitle = options.popupTitle;
    }

    // Public API
    return {
        openChatPopup: openChatPopup,
        openChatPopupWithWrapper: openChatPopupWithWrapper,
        openChatModal: openChatModal,
        closeChatModal: closeChatModal,
        configure: configure
    };
})();

// Example usage (can be removed in production)
/*
// Configure the chat URL (optional)
BlazorChatIntegration.configure({
    chatUrl: 'https://your-blazor-app-url.com',
    popupWidth: 450,
    popupHeight: 650
});

// Open chat in popup
BlazorChatIntegration.openChatPopup('user123', 'John Doe');

// Or open chat in modal
BlazorChatIntegration.openChatModal('user123', 'John Doe');
*/
