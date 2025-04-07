using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.IssuesReviews.Events;

public record IssueSentForRevisionDomainEvent(UserIssueId UserIssueId) : IDomainEvent;