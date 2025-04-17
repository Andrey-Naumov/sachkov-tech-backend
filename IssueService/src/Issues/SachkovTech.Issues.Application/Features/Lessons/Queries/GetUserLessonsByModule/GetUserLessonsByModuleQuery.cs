using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Lessons.Queries.GetUserLessonsByModule;

public record GetUserLessonsByModuleQuery(int Page, int PageSize, Guid ModuleId, Guid UserId, string? Search) : IQuery;