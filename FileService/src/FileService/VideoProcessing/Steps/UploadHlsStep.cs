using FileService.Contracts;
using FileService.FilesManagement;

namespace FileService.VideoProcessing.Steps;

public class UploadHlsStep : BaseVideoProcessingStep
{
    public override double Weight => 0.04;

    private readonly IS3Provider _s3Provider;

    protected override string StepName => "Upload HLS";


    public UploadHlsStep(IS3Provider s3Provider, ILogger<UploadHlsStep> logger)
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
            string outputPath = context.HlsOutputPath;
            string[] files = Directory.GetFiles(outputPath);
            Guid hlsGuid = Guid.NewGuid();

            var uploadTasks = files.Select(file =>
                UploadHlsFileAsync(file, hlsGuid, context.VideoLocation, cancellationToken));

            await Task.WhenAll(uploadTasks);

            context.HlsGuid = hlsGuid;

            return new StepResult(true, "HLS files uploaded successfully", hlsGuid);
        }
        catch (Exception ex)
        {
            throw new FileUploadException("Failed to upload HLS files", ex);
        }
    }

    private async Task UploadHlsFileAsync(
        string filePath,
        Guid hlsGuid,
        FileLocation location,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var fileStream = File.OpenRead(filePath);
            string fileName = Path.GetFileName(filePath);
            string fileId = Path.Combine(hlsGuid.ToString(), fileName).Replace('\\', '/');

            var fileLocation = location with
            {
                FileId = fileId
            };
            await _s3Provider.UploadFileAsync(fileLocation, null, fileStream, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new FileUploadException($"Failed to upload HLS file {filePath}", ex);
        }
    }
}