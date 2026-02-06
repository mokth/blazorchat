using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace blazorchat.Components.Shared;

public partial class MessageInput : ComponentBase
{
    private const long MaxImageBytes = 5 * 1024 * 1024;
    private const long MaxFileBytes = 20 * 1024 * 1024;
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter] public bool CanSend { get; set; } = true;
    [Parameter] public string Placeholder { get; set; } = "Type a message...";
    [Parameter] public EventCallback<string> OnSendMessage { get; set; }
    [Parameter] public EventCallback<(string imageData, string fileName)> OnSendImage { get; set; }
    [Parameter] public EventCallback<(string audioData, int duration)> OnSendVoice { get; set; }
    [Parameter] public EventCallback<(string fileData, string fileName)> OnSendFile { get; set; }
    [Parameter] public EventCallback OnTyping { get; set; }

    private string currentMessage = "";
    private ElementReference imageInputRef;
    private ElementReference fileInputRef;
    private bool isRecording = false;
    private bool isUploading = false;
    private int recordingDuration = 0;
    private System.Threading.Timer? recordingTimer;

    private async Task SendCurrentMessage()
    {
        if (!string.IsNullOrWhiteSpace(currentMessage) && CanSend)
        {
            await OnSendMessage.InvokeAsync(currentMessage);
            currentMessage = "";
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendCurrentMessage();
        }
        else if (CanSend)
        {
            // Trigger typing indicator on any key press
            await OnTyping.InvokeAsync();
        }
    }

    private async Task HandleImageUpload()
    {
        await JSRuntime.InvokeVoidAsync("triggerFileInput", imageInputRef);
    }

    private async Task HandleFileUpload()
    {
        await JSRuntime.InvokeVoidAsync("triggerFileInput", fileInputRef);
    }

    private async Task OnImageSelected(ChangeEventArgs e)
    {
        var files = await JSRuntime.InvokeAsync<string>("readFileAsBase64", imageInputRef, MaxImageBytes, "5 MB");
        if (!string.IsNullOrEmpty(files))
        {
            var parts = files.Split('|');
            if (parts.Length == 2)
            {
                await OnSendImage.InvokeAsync((parts[0], parts[1]));
            }
        }
    }

    private async Task OnFileSelected(ChangeEventArgs e)
    {
        var files = await JSRuntime.InvokeAsync<string>("readFileAsBase64", fileInputRef, MaxFileBytes, "20 MB");
        if (!string.IsNullOrEmpty(files))
        {
            var parts = files.Split('|');
            if (parts.Length == 2)
            {
                await OnSendFile.InvokeAsync((parts[0], parts[1]));
            }
        }
    }

    private async Task ToggleVoiceRecorder()
    {
        if (!isRecording)
        {
            await StartRecording();
        }
        else
        {
            await StopRecording();
        }
    }

    private async Task StartRecording()
    {
        if (isRecording)
        {
            return;
        }

        isRecording = true;
        recordingDuration = 0;

        recordingTimer = new System.Threading.Timer(_ =>
        {
            recordingDuration++;
            InvokeAsync(StateHasChanged);

            // Auto-stop after 2 minutes
            if (recordingDuration >= 120)
            {
                InvokeAsync(StopRecording);
            }
        }, null, 1000, 1000);

        await JSRuntime.InvokeVoidAsync("startVoiceRecording");
    }

    private async Task StopRecording()
    {
        if (!isRecording)
        {
            return;
        }

        isRecording = false;
        recordingTimer?.Dispose();

        var audioData = await JSRuntime.InvokeAsync<string>("stopVoiceRecording");
        if (!string.IsNullOrEmpty(audioData))
        {
            await OnSendVoice.InvokeAsync((audioData, recordingDuration));
        }

        recordingDuration = 0;
    }
}
