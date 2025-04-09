using FileService.Contracts;
using FileService.VideoProcessing.Steps;

namespace FileService.VideoProcessing;

public class VideoProcessor
{
    private readonly IEnumerable<BaseVideoProcessingStep> _processingSteps;
    private readonly ILogger<VideoProcessor> _logger;

    public VideoProcessor(
        IEnumerable<BaseVideoProcessingStep> processingSteps,
        ILogger<VideoProcessor> logger)
    {
        _processingSteps = processingSteps;
        _logger = logger;

        double totalWeight = _processingSteps.Sum(s => s.Weight);
        _logger.LogInformation("Total weight of all steps: {TotalWeight}", totalWeight);
    }

    public async Task<ProcessVideoResult> ProcessVideoAsync(
        FileLocation videoLocation,
        AsyncProgress<double> progress,
        CancellationToken cancellationToken = default)
    {
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

            if (!context.HlsGuid.HasValue || !context.PreviewGuid.HasValue)
            {
                throw new FfmpegProcessingException($"Failed to process video {videoLocation.FileId}");
            }

            await progress.ReportAsync(1.0);

            return new ProcessVideoResult(context.HlsGuid.Value, context.PreviewGuid.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video {FileId}", videoLocation.FileId);
            throw;
        }
    }
}