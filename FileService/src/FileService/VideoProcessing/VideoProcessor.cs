using FileService.Contracts;
using FileService.Hubs;
using FileService.VideoProcessing.Steps;
using Microsoft.AspNetCore.SignalR;

namespace FileService.VideoProcessing;

public class VideoProcessor
{
    private readonly IEnumerable<BaseVideoProcessingStep> _processingSteps;
    private readonly IHubContext<VideoProcessingHub, IVideoProcessingClient> _hubContext;
    private readonly ILogger<VideoProcessor> _logger;

    public VideoProcessor(
        IEnumerable<BaseVideoProcessingStep> processingSteps,
        ILogger<VideoProcessor> logger,
        IHubContext<VideoProcessingHub, IVideoProcessingClient> hubContext)
    {
        _processingSteps = processingSteps;

        double totalWeight = _processingSteps.Sum(s => s.Weight);

        _logger = logger;
        _hubContext = hubContext;

        _logger.LogInformation($"Total weight of all steps: {totalWeight}");
    }

    public async Task<ProcessVideoResult> ProcessVideoAsync(
        FileLocation videoLocation,
        CancellationToken cancellationToken = default)
    {
        var progress = new AsyncProgress<double>(async p =>
        {
            double roundedProgress = Math.Round(p * 100, 2);

            await _hubContext.Clients.Group(videoLocation.FileId).ProgressUpdate(roundedProgress, cancellationToken);

            _logger.LogInformation("Overall progress: {Progress}%", roundedProgress);
        });

        var context = new VideoProcessingContext
        {
            VideoLocation = videoLocation, Progress = progress,
        };

        try
        {
            foreach (var step in _processingSteps)
            {
                await step.ExecuteAsync(context, cancellationToken);
            }

            await context.Progress.ReportAsync(1.0);

            if (!context.HlsGuid.HasValue || !context.PreviewGuid.HasValue)
            {
                throw new FfmpegProcessingException($"Failed to process video {videoLocation.FileId}");
            }

            return new ProcessVideoResult(context.HlsGuid.Value, context.PreviewGuid.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video {FileId}", videoLocation.FileId);
            throw;
        }
    }
}