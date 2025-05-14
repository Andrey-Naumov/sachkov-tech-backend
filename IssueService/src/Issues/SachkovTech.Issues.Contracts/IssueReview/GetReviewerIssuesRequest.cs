namespace SachkovTech.Issues.Contracts.IssueReview;

public record GetReviewerIssuesRequest(string? Cursor, int Limit);