using FileService.Contracts.IntegrationEvents;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Issues.Application.Features.Lessons.Command.AddProcessedVideoToLesson;
using SachkovTech.Issues.Presentation.Hubs;

namespace SachkovTech.Issues.Infrastructure.Consumers;

public class LessonVideoProcessedConsumer : IConsumer<LessonVideoProcessedIntegrationEvent>
{
    private readonly ICommandHandler<AddProcessedVideoToLessonCommand> _handler;
    private readonly IHubContext<LessonVideoProcessingHub, ILessonVideoProcessingClient> _hubContext;
    private readonly ILogger<LessonVideoProcessedConsumer> _logger;

    public LessonVideoProcessedConsumer(
        ICommandHandler<AddProcessedVideoToLessonCommand> handler,
        IHubContext<LessonVideoProcessingHub, ILessonVideoProcessingClient> hubContext,
        ILogger<LessonVideoProcessedConsumer> logger)
    {
        _handler = handler;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LessonVideoProcessedIntegrationEvent> context)
    {
        var processedVideoEvent = context.Message;

        var command = new AddProcessedVideoToLessonCommand(
            processedVideoEvent.LessonId,
            processedVideoEvent.VideoId,
            processedVideoEvent.PreviewId);

        await _handler.Handle(command, context.CancellationToken);

        await _hubContext.Clients.Group(processedVideoEvent.LessonId.ToString())
            .LessonVideoProcessed(processedVideoEvent.LessonId, context.CancellationToken);

        _logger.LogInformation("Lesson video successfully processed: {LessonId}. Clients notified.", processedVideoEvent.LessonId);
    }
}