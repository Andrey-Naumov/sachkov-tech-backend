using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace SachkovTech.Issues.Domain.ValueObjects;

public class Video : ComparableValueObject
{
    public const string LOCATION = "videos";

    public static readonly Video None = new Video(null, null, false);

    private const long MAX_FILE_SIZE_BYTES = 5_368_709_120;
    private const string AVAILABLE_CONTENT_TYPE = "video";

    private static readonly string[] _availableExtensions =
        ["mp4", "mkv", "avi", "mov"];

    [JsonConstructor]
    private Video(Guid? originalFileId, Guid? processedFileId, bool isProcessed)
    {
        OriginalFileId = originalFileId;
        ProcessedFileId = processedFileId;
        IsProcessed = isProcessed;
    }

    public Guid? OriginalFileId { get; }

    public Guid? ProcessedFileId { get; }

    public bool IsProcessed { get; } = false;

    public string Location { get; } = LOCATION;

    public static Video CreateUnprocessed(Guid fileId) => new Video(fileId, null, false);

    public static Video CreateProcessed(Guid fileId) => new Video(null, fileId, true);

    public static UnitResult<Error> Validate(string fileName, string contentType, long size)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Errors.General.ValueIsInvalid(fileName);
        }

        int lastDotIndex = fileName.LastIndexOf('.');
        if (lastDotIndex == -1 || lastDotIndex == fileName.Length - 1)
        {
            return Errors.General.Failure();
        }

        // Извлекаем расширение без точки
        string fileExtension = fileName[(lastDotIndex + 1)..];
        if (!_availableExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
        {
            return Errors.General.Failure();
        }

        if (!contentType.Contains(AVAILABLE_CONTENT_TYPE))
        {
            return Errors.General.ValueIsInvalid(contentType);
        }

        if (size > MAX_FILE_SIZE_BYTES)
        {
            return Errors.General.Failure();
        }

        return Result.Success<Error>();
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return OriginalFileId ?? Guid.Empty;
        yield return ProcessedFileId ?? Guid.Empty;
        yield return Location;
    }
}