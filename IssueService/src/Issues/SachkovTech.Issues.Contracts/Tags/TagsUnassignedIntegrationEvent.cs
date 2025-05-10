namespace SachkovTech.Issues.Contracts.Tags;

public record TagsUnassignedIntegrationEvent(Guid EntityId, Guid[] TagIds);