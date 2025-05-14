namespace SachkovTech.Issues.Contracts.IssueReview;

public record GetUserReviewIssuesRequest(
    string? Cursor,
    int Limit);