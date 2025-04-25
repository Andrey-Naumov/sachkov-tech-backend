namespace SachkovTech.Issues.Contracts.IssueComlition;

public record GetUserCompletedIssuesRequest(
    Guid ModuleId,
    string? Cursor,
    int Limit);