using MediatR;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.IssuesComplition.DomainEvents;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.EventHandlers;

public class SendIntegrationEvent : INotificationHandler<IssueSentOnReviewEvent>
{
    private readonly IOutboxRepository _outboxRepository;

    public SendIntegrationEvent(IOutboxRepository outboxRepository)
    {
        _outboxRepository = outboxRepository;
    }

    public async Task Handle(IssueSentOnReviewEvent domainEvent, CancellationToken cancellationToken)
    {
        var integrationEvent = new Contracts.Messaging.IssueSentOnReviewEvent(
            domainEvent.UserId,
            domainEvent.UserIssueId.Value,
            Guid.Empty);

        await _outboxRepository.Add(
            integrationEvent,
            cancellationToken);
    }
}