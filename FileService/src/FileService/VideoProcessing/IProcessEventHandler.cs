namespace FileService.VideoProcessing;

public interface IProcessEventHandler
{
    Task OnProcessStartingAsync();

    Task OnOutputReceivedAsync(string data);

    Task OnProcessExecuting(string data);

    Task OnProcessExitedAsync(int exitCode);
}