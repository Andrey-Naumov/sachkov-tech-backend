namespace SachkovTech.Issues.Contracts.IssueComlition;

public record GetUserActiveIssuesRequest(
    Guid ModuleId,
    string? Cursor,
    int Limit);