using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Queries.GetUserNewIssues;

public record GetUserNewIssuesQuery(
    Guid UserId,
    Guid ModuleId,
    string? Cursor,
    int Limit) : IQuery;