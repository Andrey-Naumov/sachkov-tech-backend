using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Issues.Contracts.Tags;
using SachkovTech.Issues.Domain.Tags.Events;

namespace SachkovTech.Issues.Application.Features.Tags.EventHandlers;

public class TagsUnassignedHandler : INotificationHandler<TagsUnassignedDomainEvent>
{
    private readonly ILogger<TagsUnassignedHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public TagsUnassignedHandler(
        ILogger<TagsUnassignedHandler> logger,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(
        TagsUnassignedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var tagsUnassignedEvent = new TagsUnassignedIntegrationEvent(
            domainEvent.EntityId,
            domainEvent.TagIds);

        await _publishEndpoint.Publish(tagsUnassignedEvent, cancellationToken);

        _logger.LogInformation("Tags unassigned from entity {entityId}",
            domainEvent.EntityId);
    }
}