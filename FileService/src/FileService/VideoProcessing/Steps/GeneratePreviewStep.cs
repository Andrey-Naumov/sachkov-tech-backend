using FileService.Contracts;
using FileService.FilesManagement;
using Microsoft.Extensions.Options;

namespace FileService.VideoProcessing.Steps;

public class GeneratePreviewStep : BaseVideoProcessingStep
{
    public override double Weight => 0.04;

    private readonly IS3Provider _s3Provider;
    private readonly ProcessRunner _processRunner;
    private readonly VideoProcessOptions _videoProcessOptions;

    protected override string StepName => "Generate Preview";


    public GeneratePreviewStep(
        IS3Provider s3Provider,
        ProcessRunner processRunner,
        IOptions<VideoProcessOptions> options,
        ILogger<GeneratePreviewStep> logger)
        : base(logger)
    {
        _s3Provider = s3Provider;
        _processRunner = processRunner;
        _videoProcessOptions = options.Value;
    }

    protected override async Task<StepResult> ExecuteInternalAsync(
        VideoProcessingContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            string inputPath = context.InputPath;
            string thumbnailPath = Path.Combine(context.TempDirectory, "preview_720.jpg");

            string commandArgs =
                $"-hide_banner -y -i {inputPath} " +
                "-vf \"thumbnail,scale=1280:720:force_original_aspect_ratio=decrease,pad=1280:720:(ow-iw)/2:(oh-ih)/2\" " +
                "-frames:v 1 " +
                thumbnailPath;

            var command = new ProcessCommand(_videoProcessOptions.FfmpegPath, commandArgs);
            var result = await _processRunner.RunCommandAsync(command);

            if (result.HasError)
            {
                throw new FfmpegProcessingException($"FFmpeg thumbnail generation failed: {result.ExecutionCommandLog}");
            }

            await using var fileStream = File.OpenRead(thumbnailPath);
            Guid previewGuid = Guid.NewGuid();

            var previewLocation = new FileLocation(previewGuid.ToString(), "photos");
            await _s3Provider.UploadFileAsync(previewLocation, "image/jpeg", fileStream, cancellationToken);

            context.PreviewGuid = previewGuid;

            return new StepResult(true, "Thumbnail generated successfully", previewGuid);
        }
        catch (Exception ex) when (ex is not FfmpegProcessingException)
        {
            throw new FfmpegProcessingException("Failed to generate video thumbnail", ex);
        }
    }
}