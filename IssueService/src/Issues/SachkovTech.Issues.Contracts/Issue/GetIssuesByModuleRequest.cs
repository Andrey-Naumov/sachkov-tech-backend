namespace SachkovTech.Issues.Contracts.Issue;

public record GetIssuesByModuleRequest(
    Guid ModuleId,
    string? Title,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize);