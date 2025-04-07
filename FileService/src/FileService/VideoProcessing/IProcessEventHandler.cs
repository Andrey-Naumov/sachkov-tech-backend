namespace FileService.VideoProcessing;

public interface IProcessEventHandler
{
    Task OnProcessStartingAsync();

    Task OnOutputReceivedAsync(string data);

    Task OnErrorReceivedAsync(string data);

    Task OnProcessExitedAsync(int exitCode);
}