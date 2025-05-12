namespace SachkovTech.Issues.Contracts.ModuleComplition;

public record GetUserCompletedIssuesRequest(
    Guid ModuleId,
    string? Cursor,
    int Limit);