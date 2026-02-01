// Voice Recording functionality using MediaRecorder API

let mediaRecorder = null;
let audioChunks = [];
let startTime = null;
let recordingStream = null;

// Start recording
window.startVoiceRecording = async () => {
    try {
        // Request microphone access
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        recordingStream = stream;
        
        // Create MediaRecorder instance
        const options = { mimeType: 'audio/webm' };
        
        // Check if webm is supported, fallback to other formats
        if (!MediaRecorder.isTypeSupported('audio/webm')) {
            if (MediaRecorder.isTypeSupported('audio/mp4')) {
                options.mimeType = 'audio/mp4';
            } else if (MediaRecorder.isTypeSupported('audio/ogg')) {
                options.mimeType = 'audio/ogg';
            }
        }
        
        mediaRecorder = new MediaRecorder(stream, options);
        audioChunks = [];
        startTime = Date.now();
        
        // Handle data available event
        mediaRecorder.ondataavailable = (event) => {
            if (event.data.size > 0) {
                audioChunks.push(event.data);
            }
        };
        
        // Start recording
        mediaRecorder.start();
        
        console.log('Recording started');
        return true;
    } catch (error) {
        console.error('Error starting recording:', error);
        throw error;
    }
};

// Stop recording and return audio data
window.stopVoiceRecording = () => {
    return new Promise((resolve, reject) => {
        if (!mediaRecorder || mediaRecorder.state === 'inactive') {
            reject('No active recording');
            return;
        }
        
        mediaRecorder.onstop = async () => {
            try {
                // Calculate duration
                const duration = Math.floor((Date.now() - startTime) / 1000);
                
                // Create blob from chunks
                const audioBlob = new Blob(audioChunks, { type: mediaRecorder.mimeType });
                
                // Create object URL for preview
                const audioUrl = URL.createObjectURL(audioBlob);
                
                // Convert to base64
                const reader = new FileReader();
                reader.onloadend = () => {
                    const base64data = reader.result;
                    
                    // Stop all tracks
                    if (recordingStream) {
                        recordingStream.getTracks().forEach(track => track.stop());
                        recordingStream = null;
                    }
                    
                    resolve({
                        audioData: base64data,
                        audioUrl: audioUrl,
                        duration: duration
                    });
                };
                reader.onerror = () => {
                    reject('Error converting audio to base64');
                };
                reader.readAsDataURL(audioBlob);
                
            } catch (error) {
                console.error('Error processing recording:', error);
                reject(error);
            }
        };
        
        // Stop recording
        mediaRecorder.stop();
    });
};

// Check if voice recording is supported
window.isVoiceRecordingSupported = () => {
    return !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia && window.MediaRecorder);
};
