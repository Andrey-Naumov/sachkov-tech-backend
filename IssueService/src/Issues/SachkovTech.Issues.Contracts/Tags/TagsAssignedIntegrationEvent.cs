namespace SachkovTech.Issues.Contracts.Tags;

public record TagsAssignedIntegrationEvent(Guid EntityId, Guid[] TagIds);