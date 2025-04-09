using System.Diagnostics;
using System.Text;

namespace FileService.VideoProcessing;

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
                await eventHandler.OnProcessExecuting(e.Data);
        };

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