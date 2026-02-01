// Auto-scroll to bottom of messages
function scrollToBottom() {
    const container = document.getElementById('messagesContainer');
    if (container) {
        container.scrollTop = container.scrollHeight;
    }
}

// Call scroll on page load and when messages are added
window.addEventListener('load', () => {
    scrollToBottom();
    
    // Create a MutationObserver to watch for new messages
    const container = document.getElementById('messagesContainer');
    if (container) {
        const observer = new MutationObserver(() => {
            scrollToBottom();
        });
        
        observer.observe(container, {
            childList: true,
            subtree: true
        });
    }
});

// Read file as Base64
window.readFileAsBase64 = async (inputElement) => {
    return new Promise((resolve, reject) => {
        if (!inputElement || !inputElement.files || inputElement.files.length === 0) {
            reject('No file selected');
            return;
        }
        
        const file = inputElement.files[0];
        const reader = new FileReader();
        
        reader.onload = (e) => {
            resolve(e.target.result);
        };
        
        reader.onerror = (e) => {
            reject('Error reading file');
        };
        
        reader.readAsDataURL(file);
    });
};
