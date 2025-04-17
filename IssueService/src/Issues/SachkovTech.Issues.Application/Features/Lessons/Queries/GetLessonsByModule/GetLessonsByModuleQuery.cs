using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Lessons.Queries.GetLessonsByModule;

public record GetLessonsByModuleQuery(int Page, int PageSize, Guid ModuleId, string? Search) : IQuery;