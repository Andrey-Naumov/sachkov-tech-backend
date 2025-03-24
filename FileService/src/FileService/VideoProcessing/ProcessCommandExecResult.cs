namespace FileService.VideoProcessing;

public class ProcessCommandExecResult
{
    public string RunningCommandLog { get; set; } = string.Empty;

    public string ExecutionCommandLog { get; set; } = string.Empty;

    public bool HasError { get; set; }
}