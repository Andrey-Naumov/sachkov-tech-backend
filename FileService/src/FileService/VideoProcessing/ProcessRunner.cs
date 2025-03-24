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

    /// <summary>
    /// Запуск исполняемого файла в отдельном процессе
    /// </summary>
    /// <param name="command">Определенная команда на выполнение.</param>
    /// <returns>Результат выполнения команды.</returns>
    public async Task<ProcessCommandExecResult> RunCommandAsync(ProcessCommand command)
    {
        _logger.LogInformation("Starting process: {ExecutableFile} {Arguments}", command.ExecutableFile, command.OneLineCommandArgs);

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

        process.OutputDataReceived += (_, e) =>
        {
            runningCommandLogs.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            executionCommandLogs.AppendLine(e.Data);
        };

        process.Start();
        _logger.LogInformation("Process started with ID: {ProcessId}", process.Id);

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        int exitCode = process.ExitCode;

        _logger.LogInformation("Process exited with code: {ExitCode}", exitCode);

        process.Close();

        var result = new ProcessCommandExecResult
        {
            RunningCommandLog = runningCommandLogs.ToString(),
            ExecutionCommandLog = executionCommandLogs.ToString(),
            HasError = command.ErrorDetector?.Invoke(executionCommandLogs.ToString()) ?? false,
        };

        return result;
    }
}