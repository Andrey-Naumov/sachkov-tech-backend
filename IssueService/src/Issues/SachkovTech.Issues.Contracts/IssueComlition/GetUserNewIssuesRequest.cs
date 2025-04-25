namespace SachkovTech.Issues.Contracts.IssueComlition;

public record GetUserNewIssuesRequest(
    Guid ModuleId,
    string? Cursor,
    int Limit);