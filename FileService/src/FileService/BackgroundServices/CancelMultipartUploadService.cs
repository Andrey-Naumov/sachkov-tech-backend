using Amazon.S3;
using FileService.Contracts;
using FileService.FilesManagement;

namespace FileService.BackgroundServices;

public class CancelMultipartUploadService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<CancelMultipartUploadService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(10);
    private readonly TimeSpan _uploadTimeout = TimeSpan.FromMinutes(30);

    public CancelMultipartUploadService(IServiceProvider provider, ILogger<CancelMultipartUploadService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAndAbortUploads(stoppingToken);
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndAbortUploads(CancellationToken cancellationToken)
    {
        try
        {
            var scope = _provider.CreateScope();
            var s3Provider = scope.ServiceProvider.GetRequiredService<IS3Provider>();

            var buckets = await s3Provider.ListBucketsAsync(cancellationToken);

            foreach (var bucket in buckets)
            {
                var listMultipartUploads = await s3Provider.ListMultipartUploadAsync(bucket, cancellationToken);

                foreach (var upload in listMultipartUploads.MultipartUploads)
                {
                    if (DateTime.UtcNow - upload.Initiated > _uploadTimeout)
                    {
                        await s3Provider.AbortMultipartUploadAsync(
                            new FileLocation(upload.Key, listMultipartUploads.BucketName), upload.UploadId,
                            cancellationToken);
                    }
                }
            }
        }
        catch (AmazonS3Exception e)
        {
            _logger.LogError(e, "Error in CheckAndAbortUploads");
        }
    }
}