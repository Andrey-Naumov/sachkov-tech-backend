using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Queries.GetUserReviewIssues;

public record GetUserReviewIssuesQuery(
    Guid UserId,
    Guid ModuleId,
    string? Cursor,
    int Limit) : IQuery;