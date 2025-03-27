namespace FaqService.Contracts.Requests;

public record CreateQuestionRequest(
    string Title,
    string Description,
    string ReplLink,
    Guid UserId,
    Guid? IssueId,
    Guid? LessonId,
    List<Guid> Tags);