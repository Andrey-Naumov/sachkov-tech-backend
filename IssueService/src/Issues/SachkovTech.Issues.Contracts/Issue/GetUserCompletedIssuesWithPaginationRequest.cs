namespace SachkovTech.Issues.Contracts.Issue;

public record GetUserCompletedIssuesWithPaginationRequest(
    string? Cursor,
    int Limit);