using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Queries.GetUserCompletedIssues;

public record GetUserCompletedIssuesQuery(
    Guid UserId,
    Guid ModuleId,
    string? Cursor,
    int Limit) : IQuery;