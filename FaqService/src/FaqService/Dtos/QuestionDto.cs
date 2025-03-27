using FaqService.Contracts.Enums;

namespace FaqService.Dtos;

public record QuestionDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string PullRequestLink { get; init; } = string.Empty;
    public Guid? SolutionId { get; init; }
    public Guid UserId { get; init; }
    public Guid? IssueId { get; init; }
    public Guid? LessonId { get; init; }
    public IReadOnlyList<Guid>? Tags { get; init; }
    public Status Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public int CountOfAnswers { get; init; }
}