namespace SachkovTech.Issues.Contracts.IssueComlition;

public record GetReviewIssuesRequest(
    string? Cursor,
    int Limit);