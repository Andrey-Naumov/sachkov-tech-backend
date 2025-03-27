using FaqService.Contracts.Enums;

namespace FaqService.Contracts.Messaging.Events;

public record QuestionUpdatedEvent(
    Guid Id,
    string Title,
    string Description,
    string PullRequestLink,
    Status Status,
    DateTime CreatedAt,
    IReadOnlyList<Guid>? Tags,
    Guid? IssueId,
    Guid? LessonId);
