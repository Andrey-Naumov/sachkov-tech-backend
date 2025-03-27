using FaqService.Contracts.Enums;

namespace FaqService.Contracts.Requests;

public record GetQuestionsQuery(
    string? SearchText,
    Status? Status,
    bool? SortByDateDescending,
    Guid[] Tags,
    Guid? IssueId,
    Guid? LessonId,
    Guid? Cursor,
    int Limit = 10);