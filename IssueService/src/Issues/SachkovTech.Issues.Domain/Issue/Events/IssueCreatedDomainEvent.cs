using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.Issue.Events;

public record IssueCreatedDomainEvent(IssueId IssueId, ModuleId ModuleId) : IDomainEvent;