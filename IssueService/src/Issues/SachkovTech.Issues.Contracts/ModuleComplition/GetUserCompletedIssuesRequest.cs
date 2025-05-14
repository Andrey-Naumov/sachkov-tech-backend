namespace SachkovTech.Issues.Contracts.ModuleComplition;

public record GetUserCompletedIssuesRequest(
    string? Cursor,
    int Limit);