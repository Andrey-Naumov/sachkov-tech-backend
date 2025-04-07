using FileService.FilesManagement;

namespace FileService.VideoProcessing.Steps;

public class DownloadVideoStep : BaseVideoProcessingStep
{
    public override double Weight => 0.04;

    private readonly IS3Provider _s3Provider;

    protected override string StepName => "Download Video";


    public DownloadVideoStep(
        IS3Provider s3Provider,
        ILogger<DownloadVideoStep> logger)
        : base(logger)
    {
        _s3Provider = s3Provider;
    }

    protected override async Task<StepResult> ExecuteInternalAsync(
        VideoProcessingContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            string inputPath = await _s3Provider.DownloadFileAsync(
                context.VideoLocation,
                context.TempDirectory,
                cancellationToken);

            context.InputPath = inputPath;

            return new StepResult(true, "Video downloaded successfully");
        }
        catch (Exception ex)
        {
            throw new FileDownloadException($"Failed to download video file {context.VideoLocation.FileId}", ex);
        }
    }
}