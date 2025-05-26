namespace SachkovTech.Issues.Contracts.ModuleComplition;

public record GetReviewIssuesRequest(
    string? Cursor,
    int Limit);