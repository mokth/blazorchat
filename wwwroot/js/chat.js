// Chat JavaScript functions

// Scroll to bottom of messages
function scrollToBottom() {
    const container = document.getElementById('messagesContainer');
    if (container) {
        container.scrollTop = container.scrollHeight;
    }
}

// Auto-scroll on new messages
const observer = new MutationObserver(scrollToBottom);
const config = { childList: true, subtree: true };

window.addEventListener('DOMContentLoaded', () => {
    const container = document.getElementById('messagesContainer');
    if (container) {
        observer.observe(container, config);
        scrollToBottom();
    }
});

// Trigger file input
window.triggerFileInput = (element) => {
    if (element) {
        element.click();
    }
};

// Get selected files as base64
window.getSelectedFiles = async (input) => {
    if (!input || !input.files || input.files.length === 0) {
        return [];
    }

    const files = [];
    for (let i = 0; i < input.files.length; i++) {
        const file = input.files[i];
        const base64 = await fileToBase64(file);
        files.push(`${file.name}|${base64}`);
    }

    // Clear the input
    input.value = '';

    return files;
};

// Convert file to base64
function fileToBase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = reject;
        reader.readAsDataURL(file);
    });
}

// Check if user is at bottom of scroll
window.isAtBottom = () => {
    const container = document.getElementById('messagesContainer');
    if (!container) return true;
    
    const threshold = 50;
    const position = container.scrollTop + container.offsetHeight;
    const height = container.scrollHeight;
    
    return position >= height - threshold;
};

// Notification support
window.showNotification = (title, body) => {
    if ('Notification' in window && Notification.permission === 'granted') {
        new Notification(title, { body, icon: '/icon.png' });
    }
};

window.requestNotificationPermission = async () => {
    if ('Notification' in window && Notification.permission === 'default') {
        await Notification.requestPermission();
    }
};
