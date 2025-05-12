namespace SachkovTech.Issues.Contracts.ModuleComplition;

public record GetUserNewIssuesRequest(
    Guid ModuleId,
    string? Cursor,
    int Limit);