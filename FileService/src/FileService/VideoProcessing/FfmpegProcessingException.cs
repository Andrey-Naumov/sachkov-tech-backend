namespace FileService.VideoProcessing;

public class FfmpegProcessingException : Exception
{
    public FfmpegProcessingException(string message)
        : base(message)
    {
    }

    public FfmpegProcessingException(string message, Exception inner)
        : base(message, inner)
    {
    }
}