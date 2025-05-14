using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Queries.GetPendingReviewIssues;

public record GetPendingReviewIssuesQuery(
    string? Cursor,
    int Limit) : IQuery;