namespace SachkovTech.Issues.Contracts.IssueReview;

public record GetUserReviewIssuesRequest(
    Guid ModuleId,
    string? Cursor,
    int Limit);