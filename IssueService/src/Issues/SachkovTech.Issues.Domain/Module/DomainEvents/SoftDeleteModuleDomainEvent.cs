using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.Module.DomainEvents;

public record SoftDeleteModuleDomainEvent(ModuleId ModuleId) : IDomainEvent;