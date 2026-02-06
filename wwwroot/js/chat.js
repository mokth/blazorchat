// Chat functionality JavaScript

window.scrollToBottom = function() {
    const container = document.getElementById('messagesContainer');
    if (container) {
        container.scrollTop = container.scrollHeight;
    }
};

window.triggerFileInput = function(element) {
    if (element) {
        element.click();
    }
};

window.readFileAsBase64 = async function(inputElement) {
    return new Promise((resolve, reject) => {
        if (!inputElement || !inputElement.files || inputElement.files.length === 0) {
            resolve('');
            return;
        }

        const file = inputElement.files[0];
        const maxSize = 5 * 1024 * 1024; // 5 MB limit

        if (file.size > maxSize) {
            alert('File is too large. Maximum size is 5 MB.');
            inputElement.value = ''; // Clear the input
            resolve('');
            return;
        }

        const reader = new FileReader();

        reader.onload = function(e) {
            const base64 = e.target.result;
            const fileName = file.name;
            inputElement.value = ''; // Clear the input after reading
            resolve(base64 + '|' + fileName);
        };

        reader.onerror = function(error) {
            console.error('Error reading file:', error);
            inputElement.value = ''; // Clear the input on error
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
