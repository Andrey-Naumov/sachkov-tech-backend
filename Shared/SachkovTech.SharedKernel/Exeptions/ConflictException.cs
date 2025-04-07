namespace SharedKernel.Exeptions;

public class ConflictException : Exception
{
    public Error Error { get; }

    public ConflictException(Error error)
        : base(error.Message)
    {
        Error = error;
    }
}