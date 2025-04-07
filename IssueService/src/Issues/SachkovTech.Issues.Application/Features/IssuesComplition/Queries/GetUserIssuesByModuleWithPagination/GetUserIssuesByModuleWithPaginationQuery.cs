using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Queries.GetUserIssuesByModuleWithPagination;

public record GetUserIssuesByModuleWithPaginationQuery(
    Guid UserId,
    Guid ModuleId,
    string? Status,
    int Page,
    int PageSize) : IQuery;