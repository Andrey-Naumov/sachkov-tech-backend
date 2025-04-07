namespace SachkovTech.Issues.Contracts.Lesson;

public class LessonResponse
{
    public Guid Id { get; init; }

    public Guid ModuleId { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public int Experience { get; init; }

    public string? HlsVideoUrl { get; init; }

    public string? AutoPreviewUrl { get; init; }

    public int Position { get; init; }

    public bool IsCompleted { get; init; }

    public required Guid[] Tags { get; init; }

    public required Guid[] Issues { get; init; }

    public Guid? OriginalFileId { get; set; }

    public bool IsProcessed { get; init; }

    public Guid? ProcessedFileId { get; init; }
}