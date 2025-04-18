namespace SachkovTech.Issues.Contracts.Issue;

public record GetUserNewIssuesWithPaginationRequest(
    string? Cursor,
    int Limit);