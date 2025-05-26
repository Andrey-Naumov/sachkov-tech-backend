using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.ModulesComplition.DomainEvents;

public record IssueReturnToWorkEvent(IssueId IssueId, UserId UserId) : IDomainEvent;