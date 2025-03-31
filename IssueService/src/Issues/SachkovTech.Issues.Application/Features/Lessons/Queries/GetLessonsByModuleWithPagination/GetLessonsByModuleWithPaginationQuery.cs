using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Lessons.Queries.GetLessonsByModuleWithPagination;

public record GetLessonsByModuleQuery(int Page, int PageSize, Guid ModuleId, Guid UserId, string? Search) : IQuery;