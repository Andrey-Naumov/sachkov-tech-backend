using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FileService.VideoProcessing;

public class FfmpegProgressHandler : IProcessEventHandler
{
    private readonly ILogger _logger;
    private readonly Func<double, Task>? _progressCallback;
    private TimeSpan? _totalDuration;
    private double _localProgress;

    public FfmpegProgressHandler(ILogger logger, Func<double, Task>? progressCallback = null)
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
        if (string.IsNullOrEmpty(data)) return;

        // Парсим общую длительность из первого вывода (если еще не определена)
        if (_totalDuration == null && data.Contains("Duration:"))
        {
            _totalDuration = ParseDuration(data);
        }

        // Парсим текущий прогресс
        if (data.StartsWith("frame="))
        {
            double progress = CalculateProgress(data);
            _progressCallback?.Invoke(progress);
        }

        await Task.CompletedTask;
    }

    public async Task OnErrorReceivedAsync(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        // Парсим общую длительность из первого вывода (если еще не определена)
        if (_totalDuration == null && data.Contains("Duration:"))
        {
            _totalDuration = ParseDuration(data);
        }

        // Парсим текущий прогресс
        if (data.StartsWith("frame="))
        {
            double progress = CalculateProgress(data);
            int currentFloored = (int)Math.Floor(progress);
            int previousFloored = (int)Math.Floor(_localProgress);

            if (currentFloored - previousFloored >= 1)
            {
                _progressCallback?.Invoke(currentFloored);
            }

            _localProgress = progress;
        }

        await Task.CompletedTask;
    }

    public async Task OnProcessExitedAsync(int exitCode)
    {
        _logger.LogInformation("FFmpeg process exited with code: {ExitCode}", exitCode);
        await Task.CompletedTask;
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

public class ProcessRunner
{
    private readonly ILogger<ProcessRunner> _logger;

    public ProcessRunner(ILogger<ProcessRunner> logger)
    {
        _logger = logger;
    }

    public async Task<ProcessCommandExecResult> RunCommandAsync(
        ProcessCommand command,
        IProcessEventHandler? eventHandler = null)
    {
        _logger.LogInformation(
            "Starting process: {ExecutableFile} {Arguments}",
            command.ExecutableFile, command.OneLineCommandArgs);

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = command.ExecutableFile,
            Arguments = command.OneLineCommandArgs,
            CreateNoWindow = false,
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        process.EnableRaisingEvents = true;

        var runningCommandLogs = new StringBuilder();
        var executionCommandLogs = new StringBuilder();

        // Подписываемся на события
        process.OutputDataReceived += async (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            runningCommandLogs.AppendLine(e.Data);
            if (eventHandler != null)
                await eventHandler.OnOutputReceivedAsync(e.Data);
        };

        process.ErrorDataReceived += async (_, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            executionCommandLogs.AppendLine(e.Data);
            if (eventHandler != null)
                await eventHandler.OnErrorReceivedAsync(e.Data);
        };

        // Запускаем процесс
        if (eventHandler != null)
            await eventHandler.OnProcessStartingAsync();

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        int exitCode = process.ExitCode;

        process.Close();

        if (eventHandler != null)
            await eventHandler.OnProcessExitedAsync(exitCode);

        return new ProcessCommandExecResult
        {
            RunningCommandLog = runningCommandLogs.ToString(),
            ExecutionCommandLog = executionCommandLogs.ToString(),
            HasError = command.ErrorDetector?.Invoke(executionCommandLogs.ToString()) ?? false,
        };
    }
}