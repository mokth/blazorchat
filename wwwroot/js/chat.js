// Chat functionality JavaScript

window.scrollToBottom = function() {
    const container = document.getElementById('messagesContainer');
    if (container) {
        container.scrollTop = container.scrollHeight;
    }
};

window.triggerFileInput = function(element) {
    element.click();
};

window.readFileAsBase64 = function(element) {
    return new Promise((resolve, reject) => {
        const file = element.files[0];
        if (!file) {
            resolve('');
            return;
        }

        const reader = new FileReader();
        reader.onload = function(e) {
            const base64 = e.target.result;
            const fileName = file.name;
            resolve(base64 + '|' + fileName);
        };
        reader.onerror = function(error) {
            reject(error);
        };
        reader.readAsDataURL(file);
    });
};

// Auto-scroll on new messages
const observeMessages = function() {
    const container = document.getElementById('messagesContainer');
    if (container) {
        const observer = new MutationObserver(() => {
            window.scrollToBottom();
        });
        
        observer.observe(container, { 
            childList: true, 
            subtree: true 
        });
    }
};

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', observeMessages);
} else {
    observeMessages();
}

// Handle connection status
window.onConnectionStatusChange = function(isConnected) {
    console.log('Connection status:', isConnected ? 'Connected' : 'Disconnected');
};
