using MassTransit;
using MediatR;
using SachkovTech.Issues.Contracts.Lesson.IntegrationEvents;
using SachkovTech.Issues.Domain.Lesson;

namespace SachkovTech.Issues.Application.Features.Lessons.EventHandlers;

public class LessonVideoUploadedHandler : INotificationHandler<LessonVideoUploadedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public LessonVideoUploadedHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(LessonVideoUploadedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var lessonVideoUploadedEvent = new LessonVideoUploadedIntegrationEvent(
            domainEvent.LessonId,
            domainEvent.VideoId,
            domainEvent.FileLocation);

        await _publishEndpoint.Publish(lessonVideoUploadedEvent, cancellationToken);
    }
}