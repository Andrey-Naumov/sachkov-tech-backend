namespace SachkovTech.Issues.Contracts.Issue;

public record GetIssuesWithPaginationRequest(
    string? Title,
    int? PositionFrom,
    int? PositionTo,
    string? SortBy,
    string? SortDirection,
    int Page,
    int PageSize);