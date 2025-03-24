using FileService.Contracts;
using FileService.Contracts.IntegrationEvents;
using FileService.VideoProcessing;
using MassTransit;
using SachkovTech.Issues.Contracts.Lesson.IntegrationEvents;

namespace FileService.Consumers;

public class LessonVideoUploadedConsumer : IConsumer<LessonVideoUploadedIntegrationEvent>
{
    private readonly VideoProcessor _videoProcessor;

    public LessonVideoUploadedConsumer(VideoProcessor videoProcessor)
    {
        _videoProcessor = videoProcessor;
    }

    public async Task Consume(ConsumeContext<LessonVideoUploadedIntegrationEvent> context)
    {
        var uploadedEvent = context.Message;

        var result = await _videoProcessor.ProcessVideoAsync(
            new FileLocation(uploadedEvent.VideoId.ToString(), uploadedEvent.FileLocation),
            context.CancellationToken);

        await context.Publish(new LessonVideoProcessedIntegrationEvent(uploadedEvent.LessonId, result.HlsId, result.PreviewId));
    }
}