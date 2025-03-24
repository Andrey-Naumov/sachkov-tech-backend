namespace FileService.VideoProcessing;

public class TemporaryDirectoryException : Exception
{
    public TemporaryDirectoryException(string message)
        : base(message)
    {
    }

    public TemporaryDirectoryException(string message, Exception inner)
        : base(message, inner)
    {
    }
}