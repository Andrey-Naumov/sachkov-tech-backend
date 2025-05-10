using SharedKernel;

namespace SachkovTech.Issues.Domain.Tags.Events;

public record TagsUnassignedDomainEvent(Guid EntityId, Guid[] TagIds) : IDomainEvent;