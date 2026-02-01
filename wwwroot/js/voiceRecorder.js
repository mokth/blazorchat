// Voice recording functionality using MediaRecorder API

let mediaRecorder = null;
let audioChunks = [];

window.startVoiceRecording = async function() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        
        // Determine the best supported mime type
        let mimeType = 'audio/webm';
        if (MediaRecorder.isTypeSupported('audio/webm;codecs=opus')) {
            mimeType = 'audio/webm;codecs=opus';
        } else if (MediaRecorder.isTypeSupported('audio/ogg;codecs=opus')) {
            mimeType = 'audio/ogg;codecs=opus';
        } else if (MediaRecorder.isTypeSupported('audio/mp4')) {
            mimeType = 'audio/mp4';
        }

        mediaRecorder = new MediaRecorder(stream, { mimeType: mimeType });
        audioChunks = [];

        mediaRecorder.ondataavailable = function(event) {
            if (event.data.size > 0) {
                audioChunks.push(event.data);
            }
        };

        mediaRecorder.start();
        console.log('Voice recording started');
        return true;
    } catch (error) {
        console.error('Error starting voice recording:', error);
        alert('Could not access microphone. Please check permissions.');
        return false;
    }
};

window.stopVoiceRecording = function() {
    return new Promise((resolve, reject) => {
        if (!mediaRecorder || mediaRecorder.state === 'inactive') {
            resolve('');
            return;
        }

        mediaRecorder.onstop = async function() {
            const audioBlob = new Blob(audioChunks, { type: mediaRecorder.mimeType });
            
            // Convert blob to base64
            const reader = new FileReader();
            reader.onloadend = function() {
                const base64data = reader.result;
                
                // Stop all tracks
                mediaRecorder.stream.getTracks().forEach(track => track.stop());
                
                console.log('Voice recording stopped');
                resolve(base64data);
            };
            reader.onerror = function(error) {
                reject(error);
            };
            reader.readAsDataURL(audioBlob);
        };

        mediaRecorder.stop();
    });
};

// Cleanup function
window.cleanupVoiceRecorder = function() {
    if (mediaRecorder && mediaRecorder.stream) {
        mediaRecorder.stream.getTracks().forEach(track => track.stop());
    }
    mediaRecorder = null;
    audioChunks = [];
};
