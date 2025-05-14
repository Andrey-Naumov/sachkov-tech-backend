namespace SachkovTech.Issues.Contracts.ModuleComplition;

public record GetUserActiveIssuesRequest(
    string? Cursor,
    int Limit);