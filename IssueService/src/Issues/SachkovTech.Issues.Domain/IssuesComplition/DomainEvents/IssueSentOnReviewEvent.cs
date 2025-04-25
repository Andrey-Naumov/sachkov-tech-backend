using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.IssuesComplition.DomainEvents;

public record IssueSentOnReviewEvent(
    IssueId IssueId,
    Guid UserId,
    PullRequestUrl PullRequestUrl) : IDomainEvent;