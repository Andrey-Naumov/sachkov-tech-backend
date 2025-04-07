namespace FileService.VideoProcessing;

public class AsyncProgress<T>
{
    private readonly Func<T, Task> _progressCallback;

    public AsyncProgress(Func<T, Task> progressCallback)
    {
        _progressCallback = progressCallback;
    }

    public Task ReportAsync(T value)
    {
        return _progressCallback(value);
    }
}