using Microsoft.Extensions.Options;

namespace FileService.VideoProcessing.Steps;

public class GenerateHlsStep : BaseVideoProcessingStep
{
    public override double Weight => 0.8;

    private readonly ProcessRunner _processRunner;
    private readonly VideoProcessOptions _videoProcessOptions;

    protected override string StepName => "Generate HLS";


    public GenerateHlsStep(
        ProcessRunner processRunner,
        IOptions<VideoProcessOptions> options,
        ILogger<GenerateHlsStep> logger)
        : base(logger)
    {
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
            string outputPath = Path.Combine(context.TempDirectory, "hls_output");
            Directory.CreateDirectory(outputPath);

            double stepStartProgress = context.CurrentProgress;

            var progressHandler = new FfmpegProgressHandler(
                Logger,
                async progress =>
                {
                    double localProgress = progress / 100.0;
                    double totalProgress = stepStartProgress + (localProgress * Weight);
                    await context.Progress.ReportAsync(totalProgress);
                });

            var command = new ProcessCommand(
                _videoProcessOptions.FfmpegPath,
                $"-hide_banner -y -i {inputPath} " +
                "-filter_complex \"[0:v]split=3[v0][v1][v2]; " +
                "[v0]scale=w=-2:h=360[v0out]; " +
                "[v1]scale=w=-2:h=720[v1out]; " +
                "[v2]scale=w=-2:h=1080[v2out]\" " +
                "-map \"[v0out]\" -c:v:0 libopenh264 -profile:v:0 main -allow_skip_frames 1 -b:v:0 2M -maxrate:v:0 2M -minrate:v:0 2M -bufsize:v:0 2M -g 20 "
                +
                "-map \"[v1out]\" -c:v:1 libopenh264 -profile:v:1 main -allow_skip_frames 1 -b:v:1 3M -maxrate:v:1 3M -minrate:v:1 3M -bufsize:v:1 3M -g 20 "
                +
                "-map \"[v2out]\" -c:v:2 libopenh264 -profile:v:2 main -allow_skip_frames 1 -b:v:2 5M -maxrate:v:2 5M -minrate:v:2 5M -bufsize:v:2 5M -g 20 "
                +
                "-map a:0 -c:a:0 aac -b:a:0 64k -ac 2 " +
                "-map a:0 -c:a:1 aac -b:a:1 96k -ac 2 " +
                "-map a:0 -c:a:2 aac -b:a:2 128k -ac 2 " +
                "-f hls " +
                "-var_stream_map \"v:0,a:0,name:360p v:1,a:1,name:720p v:2,a:2,name:1080p\" " +
                "-hls_time 4 " +
                "-hls_list_size 0 " +
                "-hls_segment_type mpegts " +
                $"-hls_segment_filename {outputPath}/%v_%06d.ts " +
                "-master_pl_name master.m3u8 " +
                $"{outputPath}/%v_stream.m3u8",
                FfmpegErrorDetectors.HlsErrorDetector);

            var result = await _processRunner.RunCommandAsync(command, progressHandler);
            if (result.HasError)
            {
                throw new FfmpegProcessingException($"FFmpeg HLS generation failed: {result.ExecutionCommandLog}");
            }

            context.HlsOutputPath = outputPath;
            return new StepResult(true, "HLS generated successfully");
        }
        catch (Exception ex) when (ex is not FfmpegProcessingException)
        {
            throw new FfmpegProcessingException("Failed to generate HLS streams", ex);
        }
    }
}