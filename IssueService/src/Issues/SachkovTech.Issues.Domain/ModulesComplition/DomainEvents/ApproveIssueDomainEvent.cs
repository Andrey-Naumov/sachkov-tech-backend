using SharedKernel;

namespace SachkovTech.Issues.Domain.ModulesComplition.DomainEvents;

public record ApproveIssueDomainEvent(Guid UserId, Guid IssueId) : IDomainEvent;