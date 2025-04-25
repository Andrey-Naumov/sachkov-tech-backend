using SachkovTech.Core.Abstractions;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Queries.GetUserActiveIssues;

public record GetUserActiveIssuesQuery(
    Guid UserId,
    Guid ModuleId,
    string? Cursor,
    int Limit) : IQuery;