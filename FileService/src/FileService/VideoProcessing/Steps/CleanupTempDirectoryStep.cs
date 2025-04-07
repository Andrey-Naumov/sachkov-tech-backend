namespace FileService.VideoProcessing.Steps;

public class CleanupTempDirectoryStep : BaseVideoProcessingStep
{
    public override double Weight => 0.04;


    protected override string StepName => "Cleanup Temp Directory";


    public CleanupTempDirectoryStep(ILogger<CleanupTempDirectoryStep> logger)
        : base(logger)
    {
    }

    protected override Task<StepResult> ExecuteInternalAsync(
        VideoProcessingContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(context.TempDirectory) && Directory.Exists(context.TempDirectory))
            {
                Directory.Delete(context.TempDirectory, true);
            }

            return Task.FromResult(new StepResult(true, "Temporary directory cleaned up successfully"));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to clean up temporary directory {TempDir}", context.TempDirectory);
            return Task.FromResult(new StepResult(false, "Failed to clean up temporary directory"));
        }
    }
}