using SharedKernel;

namespace SachkovTech.Issues.Domain.Tags.Events;

public record TagsAssignedDomainEvent(Guid EntityId, Guid[] TagIds) : IDomainEvent;