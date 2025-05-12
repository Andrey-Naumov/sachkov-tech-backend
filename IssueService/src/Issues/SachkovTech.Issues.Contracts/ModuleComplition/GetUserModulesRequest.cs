namespace SachkovTech.Issues.Contracts.ModuleComplition;

public record GetUserModulesRequest(
    string? Cursor,
    int Limit);