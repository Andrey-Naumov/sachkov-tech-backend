namespace SachkovTech.Issues.Contracts.Lesson;

public record GetLessonsRequest(int Page, int PageSize, Guid ModuleId, string? Search);