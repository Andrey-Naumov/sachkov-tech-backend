using FileService.Contracts;
using FileService.Contracts.IntegrationEvents;
using FileService.VideoProcessing;
using MassTransit;
using SachkovTech.Issues.Contracts.Lesson.IntegrationEvents;

namespace FileService.Consumers;

public class LessonVideoUploadedConsumer : IConsumer<LessonVideoUploadedIntegrationEvent>
{
    private readonly VideoProcessor _videoProcessor;
    private readonly ILogger<LessonVideoUploadedConsumer> _logger;

    public LessonVideoUploadedConsumer(
        VideoProcessor videoProcessor,
        ILogger<LessonVideoUploadedConsumer> logger)
    {
        _videoProcessor = videoProcessor;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LessonVideoUploadedIntegrationEvent> context)
    {
        var lessonVideo = context.Message;
        string fileId = lessonVideo.VideoId.ToString();

        var progress = new AsyncProgress<double>(async p =>
        {
            double roundedProgress = Math.Round(p * 100, 2);

            await context.Publish(new LessonVideoProgressReceivedIntegrationEvent(lessonVideo.LessonId, roundedProgress));

            _logger.LogInformation("Progress for lesson {LessonId}: {Progress}%", lessonVideo.LessonId, roundedProgress);
        });

        var result = await _videoProcessor.ProcessVideoAsync(
            new FileLocation(fileId, lessonVideo.FileLocation),
            progress,
            context.CancellationToken);

        await context.Publish(new LessonVideoProcessedIntegrationEvent(
            lessonVideo.LessonId,
            result.HlsId,
            result.PreviewId));
    }
}