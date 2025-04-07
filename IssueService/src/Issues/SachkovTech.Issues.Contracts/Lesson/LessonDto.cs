namespace SachkovTech.Issues.Contracts.Lesson;

public class LessonDto
{
    public Guid Id { get; init; }

    public Guid ModuleId { get; init; }

    public int Position { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public int Experience { get; init; }

    public string? HlsVideoUrl { get; set; } = string.Empty;

    public Guid AutoPreviewId { get; init; }

    public string? AutoPreviewUrl { get; set; } = string.Empty;

    public Guid ProcessedFileId { get; init; }

    public string ProcessedVideoLocation { get; init; } = string.Empty;

    public bool IsProcessed { get; init; }

    public Guid OriginalFileId { get; init; }

    public bool IsCompleted { get; init; }

    public Guid[] Tags { get; init; } = [];

    public Guid[] Issues { get; init; } = [];
}