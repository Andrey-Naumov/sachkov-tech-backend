using System.Text.RegularExpressions;

namespace FileService.VideoProcessing;

public class FfmpegProgressHandler : IProcessEventHandler
{
    private readonly ILogger _logger;
    private readonly Func<double, Task> _progressCallback;
    private TimeSpan? _totalDuration;
    private double _localProgress;

    public FfmpegProgressHandler(ILogger logger, Func<double, Task> progressCallback)
    {
        _logger = logger;
        _progressCallback = progressCallback;
    }

    public async Task OnProcessStartingAsync()
    {
        _logger.LogInformation("FFmpeg process starting");
        _localProgress = 0.0;

        await Task.CompletedTask;
    }

    public async Task OnOutputReceivedAsync(string data)
    {
        await ParseFfmpegOutput(data);
    }

    public async Task OnProcessExecuting(string data)
    {
        await ParseFfmpegOutput(data);
    }

    public async Task OnProcessExitedAsync(int exitCode)
    {
        _logger.LogInformation("FFmpeg process exited with code: {ExitCode}", exitCode);
        await Task.CompletedTask;
    }

    private async Task ParseFfmpegOutput(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        if (_totalDuration == null && data.Contains("Duration:"))
        {
            _totalDuration = ParseDuration(data);
        }

        if (data.StartsWith("frame="))
        {
            double progress = CalculateProgress(data);
            int currentFloored = (int)Math.Floor(progress);
            int previousFloored = (int)Math.Floor(_localProgress);

            if (currentFloored - previousFloored >= 1)
            {
                await _progressCallback.Invoke(currentFloored);
            }

            _localProgress = progress;
        }
    }

    private TimeSpan? ParseDuration(string data)
    {
        var match = Regex.Match(data, @"Duration: (\d{2}):(\d{2}):(\d{2})\.\d+");
        if (match.Success)
        {
            return new TimeSpan(
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value),
                int.Parse(match.Groups[3].Value));
        }

        return null;
    }

    private double CalculateProgress(string data)
    {
        if (_totalDuration == null) return 0;

        var timeMatch = Regex.Match(data, @"time=(\d{2}):(\d{2}):(\d{2})\.\d+");
        if (!timeMatch.Success)
            return 0;

        var currentTime = new TimeSpan(
            int.Parse(timeMatch.Groups[1].Value),
            int.Parse(timeMatch.Groups[2].Value),
            int.Parse(timeMatch.Groups[3].Value));

        return Math.Min(100, currentTime.TotalSeconds / _totalDuration.Value.TotalSeconds * 100);
    }
}