using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Issue.Queries.GetUserNewIssues;

public record GetUserNewIssuesWithPaginationQuery(
    Guid UserId,
    string? Cursor,
    int Limit) : IQuery;