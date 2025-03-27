using Nest;

namespace FaqService.Entities.Elastic;

public class QuestionElastic
{
    [Keyword(Name = "id")]
    public Guid Id { get; init; }

    [Text(Name = "title")]
    public string Title { get; init; } = string.Empty;

    [Text(Name = "description")]
    public string Description { get; init; } = string.Empty;

    [Text(Name = "pullRequestLink")]
    public string PullRequestLink { get; init; } = string.Empty;

    [Keyword(Name = "status")]
    public string Status { get; init; } = string.Empty;

    [Date(Name = "createdAt")]
    public DateTime CreatedAt { get; init; }

    [Keyword(Name = "tags")]
    public IReadOnlyList<Guid> Tags { get; init; } = [];

    [Keyword(Name = "issueId")]
    public Guid? IssueId { get; init; }

    [Keyword(Name = "lessonId")]
    public Guid? LessonId { get; init; }
}