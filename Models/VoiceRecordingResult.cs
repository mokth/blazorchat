namespace blazorchat.Models;

public class VoiceRecordingResult
{
    public string AudioData { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    public int Duration { get; set; }
}
