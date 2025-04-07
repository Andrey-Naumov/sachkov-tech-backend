using CSharpFunctionalExtensions;
using SharedKernel;

namespace SachkovTech.Issues.Domain.Lesson.ValueObjects;

public class Preview : ComparableValueObject
{
    public const string LOCATION = "photos";

    private const long MAX_FILE_SIZE = 10485760;

    public static readonly Preview None = new(Guid.Empty);

    private static readonly string[] _permitedFilesType = ["image/jpg", "image/jpeg", "image/png", "image/gif"];
    private static readonly string[] _permitedExtensions = ["jpg", "jpeg", "png", "gif"];

    public Preview(Guid fileId)
    {
        FileId = fileId;
    }

    public Guid FileId { get; }

    public string FileLocation { get; } = LOCATION;

    public static UnitResult<Error> Validate(
        string fileName,
        string contentType,
        long size)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Errors.General.ValueIsInvalid(fileName);
        }

        string? fileExtension = fileName[fileName.LastIndexOf('.')..];

        if (_permitedExtensions.All(x => x != fileExtension))
        {
            return Errors.General.Failure();
        }

        if (_permitedFilesType.All(x => x != contentType))
        {
            return Errors.General.ValueIsInvalid(contentType);
        }

        if (size > MAX_FILE_SIZE)
        {
            return Errors.General.Failure();
        }

        return Result.Success<Error>();
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return FileId;
    }
}