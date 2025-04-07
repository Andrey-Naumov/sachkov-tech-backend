using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.IssuesComplition.DomainEvents;

public record IssueReviewApprovedDomainEvent(UserId UserId, IssueId IssueId) : IDomainEvent;