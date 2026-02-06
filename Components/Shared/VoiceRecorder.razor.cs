using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace blazorchat.Components.Shared;

public partial class VoiceRecorder : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter] public EventCallback<(string audioData, int duration)> OnVoiceRecorded { get; set; }

    private bool IsRecording = false;
    private string recordingTime = "0:00";
    private int recordingSeconds = 0;
    private System.Threading.Timer? timer;

    private async Task StartRecording()
    {
        IsRecording = true;
        recordingSeconds = 0;

        timer = new System.Threading.Timer(_ =>
        {
            recordingSeconds++;
            recordingTime = $"{recordingSeconds / 60}:{recordingSeconds % 60:D2}";
            InvokeAsync(StateHasChanged);
        }, null, 0, 1000);

        await JSRuntime.InvokeVoidAsync("startVoiceRecording");
    }

    private async Task StopRecording()
    {
        IsRecording = false;
        timer?.Dispose();

        var audioData = await JSRuntime.InvokeAsync<string>("stopVoiceRecording");
        if (!string.IsNullOrEmpty(audioData))
        {
            await OnVoiceRecorded.InvokeAsync((audioData, recordingSeconds));
        }

        recordingSeconds = 0;
        recordingTime = "0:00";
    }
}
