namespace SachkovTech.Issues.Contracts.IssueReview;

public class IssueReviewDto
{
    public Guid IssueId { get; init; }

    public Guid? ModuleId { get; init; }

    public Guid? LessonId { get; init; }

    public Guid UserId { get; init; }

    public Guid IssueReviewId { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string Status { get; init; }

    public required string PullRequestUrl { get; init; }

    public int Position { get; init; }
}