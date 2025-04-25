using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Queries.GetUserReviewIssues;

public record GetUserReviewIssuesWithPaginationQuery(
    Guid UserId,
    Guid ModuleId,
    string? Cursor,
    int Limit) : IQuery;