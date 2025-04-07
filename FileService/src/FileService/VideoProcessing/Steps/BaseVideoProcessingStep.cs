namespace FileService.VideoProcessing.Steps;

public abstract class BaseVideoProcessingStep
{
    protected readonly ILogger Logger;

    protected BaseVideoProcessingStep(ILogger logger)
    {
        Logger = logger;
    }

    public abstract double Weight { get; }

    protected abstract string StepName { get; }

    public async Task<StepResult> ExecuteAsync(
        VideoProcessingContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            Logger.LogInformation("Starting {StepName} for video {FileId}", StepName, context.VideoLocation.FileId);

            var result = await ExecuteInternalAsync(context, cancellationToken);

            // Обновляем общий прогресс
            context.CurrentProgress += Weight;
            await context.Progress.ReportAsync(context.CurrentProgress);

            Logger.LogInformation("Completed {StepName} for video {FileId}", StepName, context.VideoLocation.FileId);

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in {StepName} for video {FileId}", StepName, context.VideoLocation.FileId);
            throw;
        }
    }

    protected abstract Task<StepResult> ExecuteInternalAsync(
        VideoProcessingContext context,
        CancellationToken cancellationToken);
}