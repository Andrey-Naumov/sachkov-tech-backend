using FileService.Contracts.IntegrationEvents;
using MassTransit;
using SachkovTech.Core.Abstractions;
using SachkovTech.Issues.Application.Features.Lessons.Command.AddProcessedVideoToLesson;

namespace SachkovTech.Issues.Infrastructure.Consumers;

public class LessonVideoProcessedConsumer : IConsumer<LessonVideoProcessedIntegrationEvent>
{
    private readonly ICommandHandler<AddProcessedVideoToLessonCommand> _handler;

    public LessonVideoProcessedConsumer(ICommandHandler<AddProcessedVideoToLessonCommand> handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<LessonVideoProcessedIntegrationEvent> context)
    {
        var processedVideoEvent = context.Message;

        var command = new AddProcessedVideoToLessonCommand(processedVideoEvent.LessonId, processedVideoEvent.VideoId, processedVideoEvent.PreviewId);
        await _handler.Handle(command, context.CancellationToken);
    }
}