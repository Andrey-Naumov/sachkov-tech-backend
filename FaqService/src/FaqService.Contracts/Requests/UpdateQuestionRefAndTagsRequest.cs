namespace FaqService.Contracts.Requests;

public record UpdateQuestionRefAndTagsRequest(
    string ReplLink,
    Guid? IssueId,
    Guid? LessonId,
    List<Guid> Tags);