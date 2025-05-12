namespace SachkovTech.Issues.Contracts.ModuleComplition;

public record GetUserActiveIssuesRequest(
    Guid ModuleId,
    string? Cursor,
    int Limit);