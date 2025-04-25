using SachkovTech.Core.Abstractions;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.Application.Features.Issue.Queries.GetIssueById;

public record GetIssueByIdQuery(IssueId IssueId, Guid UserId) : IQuery;