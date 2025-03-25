using CSharpFunctionalExtensions;
using SharedKernel;

namespace AccountService.Domain;

public class Avatar : ComparableValueObject
{
    public const string LOCATION = "photos";

    public static readonly Avatar None = new(Guid.Empty);

    private const long MAX_FILE_SIZE = 10_485_760;

    private static readonly string[] _permitedFilesType =
    [
        "image/jpg", "image/jpeg", "image/png", "image/gif"
    ];

    private static readonly string[] _permitedExtensions =
    [
        "jpg", "jpeg", "png", "gif"
    ];

    public Avatar(Guid fileId)
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
            return Errors.General.ValueIsInvalid(fileName);

        int lastDotIndex = fileName.LastIndexOf('.');
        if (lastDotIndex == -1 || lastDotIndex == fileName.Length - 1)
        {
            return Errors.General.Failure();
        }

        string fileExtension = fileName[(lastDotIndex + 1)..];

        if (_permitedExtensions.All(x => x != fileExtension))
            return Error.Validation("file.invalidExtension", "Неверное расширение файла.");

        if (_permitedFilesType.All(x => x != contentType))
            return Errors.General.ValueIsInvalid(contentType);

        if (size > MAX_FILE_SIZE)
            return Error.Validation("file.invalidSize", "Неверный размер файла.");

        return Result.Success<Error>();
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return FileId;
        yield return FileLocation;
    }
}