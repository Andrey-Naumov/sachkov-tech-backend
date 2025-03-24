using FileService.Extensions;

namespace FileService.VideoProcessing;

public class ProcessCommand
{
    /// <summary>
    /// Имя исполняемого файла программы (FFmpeg)
    /// </summary>
    public string ExecutableFile { get; }

    /// <summary>
    /// Исполняемая команда
    /// </summary>
    public string CommandArgs { get; }

    /// <summary>
    /// Функция, анализирующая логи выполнения процесса и идентифицирующая наличие ошибки, при выполнении команды
    /// </summary>
    public Func<string, bool>? ErrorDetector { get; }

    public ProcessCommand(string executableFile, string commandArgs, Func<string, bool>? errorDetector = null)
    {
        ExecutableFile = executableFile;
        CommandArgs = commandArgs;
        ErrorDetector = errorDetector;
    }

    public string OneLineCommandArgs => CommandArgs.NormalizeView();

    public string CommandView => $"{ExecutableFile} {CommandArgs.NormalizeView(Environment.NewLine)}";

    public override string ToString() => CommandView;
}