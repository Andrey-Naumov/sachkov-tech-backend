using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Queries.GetReviewerIssues;

public record GetReviewerIssuesQuery(
    Guid ReviewerId,
    string? Cursor,
    int Limit) : IQuery;