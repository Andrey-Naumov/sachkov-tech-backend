using FileService.Contracts;
using FileService.FilesManagement;
using Microsoft.Extensions.Options;

namespace FileService.VideoProcessing;

public class VideoProcessor
{
    private readonly IS3Provider _s3Provider;
    private readonly ProcessRunner _processRunner;
    private readonly VideoProcessOptions _videoProcessOptions;
    private readonly ILogger<VideoProcessor> _logger;

    public VideoProcessor(
        IS3Provider s3Provider,
        ProcessRunner processRunner,
        IOptions<VideoProcessOptions> videoProcessOptions,
        ILogger<VideoProcessor> logger)
    {
        _s3Provider = s3Provider;
        _processRunner = processRunner;
        _videoProcessOptions = videoProcessOptions.Value;
        _logger = logger;
    }

    public async Task<ProcessVideoResult> ProcessVideoAsync(FileLocation videoLocation, CancellationToken cancellationToken)
    {
        string tempDir = string.Empty;
        try
        {
            tempDir = CreateTempDirectory(videoLocation.FileId);
            string outputPath = Path.Combine(tempDir, "output");
            Directory.CreateDirectory(outputPath);

            string inputPath = await DownloadVideoFileAsync(videoLocation, tempDir, cancellationToken);

            await GenerateHlsAsync(inputPath, outputPath);

            Guid hlsGuid = await UploadHlsFilesAsync(outputPath, videoLocation, cancellationToken);

            Guid previewGuid = await GenerateThumbnailAsync(inputPath, tempDir, cancellationToken);

            return new ProcessVideoResult(hlsGuid, previewGuid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video {FileId}", videoLocation.FileId);
            throw;
        }
        finally
        {
            CleanupTempDirectory(tempDir);
        }
    }

    private string CreateTempDirectory(string fileId)
    {
        try
        {
            string baseDir = AppContext.BaseDirectory;
            string tempDir = Path.Combine(baseDir, "temp-videos", fileId);
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }
        catch (Exception ex)
        {
            throw new TemporaryDirectoryException("Failed to create temporary directory", ex);
        }
    }

    private async Task<string> DownloadVideoFileAsync(FileLocation videoLocation, string tempDir, CancellationToken cancellationToken)
    {
        try
        {
            return await _s3Provider.DownloadFileAsync(videoLocation, tempDir, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new FileDownloadException($"Failed to download video file {videoLocation.FileId}", ex);
        }
    }

    private async Task GenerateHlsAsync(string inputPath, string outputPath)
    {
        try
        {
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
                "-hls_time 2 " +
                "-hls_list_size 0 " +
                "-hls_segment_type mpegts " +
                $"-hls_segment_filename {outputPath}/%v_%06d.ts " +
                "-master_pl_name master.m3u8 " +
                $"{outputPath}/%v_stream.m3u8");

            var result = await _processRunner.RunCommandAsync(command);
            if (result.HasError)
            {
                throw new FfmpegProcessingException($"FFmpeg HLS generation failed: {result.ExecutionCommandLog}");
            }

            _logger.LogInformation("FFmpeg HLS generation succeeded: {ExecutionCommandLog}", result.ExecutionCommandLog);
        }
        catch (Exception ex) when (ex is not FfmpegProcessingException)
        {
            throw new FfmpegProcessingException("Failed to generate HLS streams", ex);
        }
    }

    private async Task<Guid> UploadHlsFilesAsync(string outputPath, FileLocation location, CancellationToken cancellationToken)
    {
        try
        {
            string[] files = Directory.GetFiles(outputPath);
            Guid hlsGuid = Guid.NewGuid();

            var uploadTasks = files.Select(file => UploadHlsFileAsync(file, hlsGuid, location, cancellationToken));
            await Task.WhenAll(uploadTasks);

            return hlsGuid;
        }
        catch (Exception ex)
        {
            throw new FileUploadException("Failed to upload HLS files", ex);
        }
    }

    private async Task UploadHlsFileAsync(string filePath, Guid hlsGuid, FileLocation location, CancellationToken cancellationToken)
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

    private async Task<Guid> GenerateThumbnailAsync(string inputPath, string tempDir, CancellationToken cancellationToken)
    {
        try
        {
            const string contentType = "image/jpeg";
            const string bucketName = "photos";

            string thumbnailPath = Path.Combine(tempDir, "preview_720.jpg");

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

            _logger.LogInformation("FFmpeg thumbnail generation succeeded: {ExecutionCommandLog}", result.ExecutionCommandLog);

            await using var fileStream = File.OpenRead(thumbnailPath);
            Guid previewGuid = Guid.NewGuid();

            var previewLocation = new FileLocation(previewGuid.ToString(), bucketName);
            await _s3Provider.UploadFileAsync(previewLocation, contentType, fileStream, cancellationToken);

            return previewGuid;
        }
        catch (Exception ex) when (ex is not FfmpegProcessingException)
        {
            throw new FfmpegProcessingException("Failed to generate video thumbnail", ex);
        }
    }

    private void CleanupTempDirectory(string tempDir)
    {
        try
        {
            if (!string.IsNullOrEmpty(tempDir) && Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clean up temporary directory {TempDir}", tempDir);
        }
    }
}