using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SachkovTech.Issues.Contracts.Tags;
using SachkovTech.Issues.Domain.Tags.Events;

namespace SachkovTech.Issues.Application.Features.Tags.EventHandlers;

public class TagsAssignedHandler : INotificationHandler<TagsAssignedDomainEvent>
{
    private readonly ILogger<TagsAssignedHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public TagsAssignedHandler(
        ILogger<TagsAssignedHandler> logger,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(
        TagsAssignedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var tagsAssignedEvent = new TagsAssignedIntegrationEvent(
            domainEvent.EntityId,
            domainEvent.TagIds);

        await _publishEndpoint.Publish(tagsAssignedEvent, cancellationToken);

        _logger.LogInformation("Tags assigned to entity {entityId}",
            domainEvent.EntityId);
    }
}