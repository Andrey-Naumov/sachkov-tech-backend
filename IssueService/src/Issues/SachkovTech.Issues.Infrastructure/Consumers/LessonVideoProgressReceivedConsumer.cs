using FileService.Contracts.IntegrationEvents;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using SachkovTech.Issues.Presentation.Hubs;

namespace SachkovTech.Issues.Infrastructure.Consumers;

public class LessonVideoProgressReceivedConsumer : IConsumer<LessonVideoProgressReceivedIntegrationEvent>
{
    private readonly IHubContext<LessonVideoProcessingHub, ILessonVideoProcessingClient> _hubContext;

    public LessonVideoProgressReceivedConsumer(IHubContext<LessonVideoProcessingHub, ILessonVideoProcessingClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<LessonVideoProgressReceivedIntegrationEvent> context)
    {
        var progressEvent = context.Message;

        await _hubContext.Clients.Group(progressEvent.LessonId.ToString())
            .ProgressUpdate(progressEvent.Progress, context.CancellationToken);
    }
}