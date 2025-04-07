namespace FileService.VideoProcessing.Steps;

public class CreateTempDirectoryStep : BaseVideoProcessingStep
{
    public override double Weight => 0.04;

    protected override string StepName => "Create Temp Directory";


    public CreateTempDirectoryStep(ILogger<CreateTempDirectoryStep> logger)
        : base(logger)
    {
    }

    protected override Task<StepResult> ExecuteInternalAsync(
        VideoProcessingContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            string baseDir = AppContext.BaseDirectory;
            string tempDir = Path.Combine(baseDir, "temp-videos", context.VideoLocation.FileId);

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }

            Directory.CreateDirectory(tempDir);
            context.TempDirectory = tempDir;

            return Task.FromResult(new StepResult(true, "Temporary directory created successfully"));
        }
        catch (Exception ex)
        {
            throw new TemporaryDirectoryException("Failed to create temporary directory", ex);
        }
    }
}