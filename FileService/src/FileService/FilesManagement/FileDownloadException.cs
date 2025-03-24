namespace FileService.FilesManagement;

public class FileDownloadException : Exception
{
    public FileDownloadException(string message)
        : base(message)
    {
    }

    public FileDownloadException(string message, Exception inner)
        : base(message, inner)
    {
    }
}