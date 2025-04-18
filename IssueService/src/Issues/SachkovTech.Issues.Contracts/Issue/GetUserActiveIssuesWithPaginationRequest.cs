namespace SachkovTech.Issues.Contracts.Issue;

public record GetUserActiveIssuesWithPaginationRequest(
    string? Cursor,
    int Limit);