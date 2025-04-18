using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Issue.Queries.GetUserCompletedIssues;

public record GetUserCompletedIssuesWithPaginationQuery(
    Guid UserId,
    string? Cursor,
    int Limit) : IQuery;