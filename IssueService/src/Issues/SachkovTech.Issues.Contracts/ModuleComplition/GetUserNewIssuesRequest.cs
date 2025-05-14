namespace SachkovTech.Issues.Contracts.ModuleComplition;

public record GetUserNewIssuesRequest(
    string? Cursor,
    int Limit);