using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.Issue.Queries.GetUserActiveIssues;

public record GetUserActiveIssuesWithPaginationQuery(
    Guid UserId,
    string? Cursor,
    int Limit) : IQuery;