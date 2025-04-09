namespace FileService.VideoProcessing;

public static class FfmpegErrorDetectors
{
    /// <summary>
    /// Стандартный детектор ошибок FFmpeg, который ищет критические ошибки в выводе
    /// </summary>
    public static readonly Func<string, bool> DefaultErrorDetector = (output) =>
    {
        if (string.IsNullOrWhiteSpace(output))
            return false;

        // Критические ошибки FFmpeg
        if (output.Contains("Error while decoding stream") ||
            output.Contains("Conversion failed") ||
            output.Contains("Invalid data found when processing input") ||
            output.Contains("Could not open file") ||
            output.Contains("No such file or directory") ||
            output.Contains("Permission denied") ||
            output.Contains("Unsupported codec") ||
            output.Contains("Invalid argument") ||
            output.Contains("Stream mapping error") ||
            output.Contains("Could not write header for output file"))
        {
            return true;
        }

        // Ошибки лицензирования или отсутствия кодеков
        if (output.Contains("codec not currently supported in container") ||
            output.Contains("encoder not found") ||
            output.Contains("Unknown encoder") ||
            output.Contains("Experimental feature"))
        {
            return true;
        }

        // Предупреждения не считаем ошибками
        return false;
    };

    /// <summary>
    /// Детектор ошибок для генерации HLS
    /// </summary>
    public static readonly Func<string, bool> HlsErrorDetector = (output) =>
    {
        if (DefaultErrorDetector(output))
            return true;

        // Специфичные ошибки для HLS
        if (output.Contains("Invalid segment filename template") ||
            output.Contains("Could not open segment file") ||
            output.Contains("Failed to open segment") ||
            output.Contains("Could not write segment"))
        {
            return true;
        }

        return false;
    };

    /// <summary>
    /// Детектор ошибок для извлечения превью
    /// </summary>
    public static readonly Func<string, bool> PreviewErrorDetector = (output) =>
    {
        if (DefaultErrorDetector(output))
            return true;

        // Специфичные ошибки для генерации превью
        if (output.Contains("Could not generate thumbnail") ||
            output.Contains("No video stream found") ||
            output.Contains("Unable to extract frame"))
        {
            return true;
        }

        return false;
    };
}