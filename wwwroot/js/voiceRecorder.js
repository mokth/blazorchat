// Voice Recorder using MediaRecorder API

let mediaRecorder = null;
let audioChunks = [];
let audioStream = null;

// Start voice recording
window.startVoiceRecording = async () => {
    try {
        // Request microphone access
        audioStream = await navigator.mediaDevices.getUserMedia({ audio: true });
        
        // Create MediaRecorder
        const options = { mimeType: 'audio/webm' };
        if (!MediaRecorder.isTypeSupported(options.mimeType)) {
            options.mimeType = 'audio/ogg';
        }
        
        mediaRecorder = new MediaRecorder(audioStream, options);
        audioChunks = [];

        // Collect audio data
        mediaRecorder.ondataavailable = (event) => {
            if (event.data.size > 0) {
                audioChunks.push(event.data);
            }
        };

        // Start recording
        mediaRecorder.start();
        
        return true;
    } catch (error) {
        console.error('Error starting voice recording:', error);
        alert('Could not access microphone. Please check permissions.');
        return false;
    }
};

// Stop voice recording and return base64 data
window.stopVoiceRecording = () => {
    return new Promise((resolve, reject) => {
        if (!mediaRecorder) {
            resolve(null);
            return;
        }

        mediaRecorder.onstop = async () => {
            try {
                // Create blob from chunks
                const audioBlob = new Blob(audioChunks, { type: mediaRecorder.mimeType });
                
                // Convert to base64
                const base64 = await blobToBase64(audioBlob);
                
                // Stop audio stream
                if (audioStream) {
                    audioStream.getTracks().forEach(track => track.stop());
                    audioStream = null;
                }
                
                // Reset recorder
                mediaRecorder = null;
                audioChunks = [];
                
                resolve(base64);
            } catch (error) {
                console.error('Error processing voice recording:', error);
                reject(error);
            }
        };

        mediaRecorder.stop();
    });
};

// Convert blob to base64
function blobToBase64(blob) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onloadend = () => resolve(reader.result);
        reader.onerror = reject;
        reader.readAsDataURL(blob);
    });
}

// Check if voice recording is supported
window.isVoiceRecordingSupported = () => {
    return !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia && window.MediaRecorder);
};

// Cancel recording
window.cancelVoiceRecording = () => {
    if (mediaRecorder && mediaRecorder.state !== 'inactive') {
        mediaRecorder.stop();
    }
    
    if (audioStream) {
        audioStream.getTracks().forEach(track => track.stop());
        audioStream = null;
    }
    
    mediaRecorder = null;
    audioChunks = [];
};
