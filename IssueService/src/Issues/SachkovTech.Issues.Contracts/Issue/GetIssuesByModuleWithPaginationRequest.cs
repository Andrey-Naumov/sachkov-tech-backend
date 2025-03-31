namespace SachkovTech.Issues.Contracts.Issue;

public record GetIssuesByModuleWithPaginationRequest(
    Guid ModuleId,
    string? Title,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize);