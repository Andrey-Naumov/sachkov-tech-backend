using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Issue.Queries.GetIssuesByModule;

public record GetFilteredIssuesByModuleQuery(
    Guid ModuleId,
    string? Title,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize) : IQuery;