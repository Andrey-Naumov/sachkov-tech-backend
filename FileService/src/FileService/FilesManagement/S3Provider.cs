using Amazon.S3;
using Amazon.S3.Model;
using FileService.Contracts;
using CompleteMultipartUploadRequest = Amazon.S3.Model.CompleteMultipartUploadRequest;

namespace FileService.FilesManagement;

public class S3Provider : IS3Provider
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Provider> _logger;

    public S3Provider(IAmazonS3 s3Client, ILogger<S3Provider> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<List<string>> ListBucketsAsync(CancellationToken cancellationToken)
    {
        var response = await _s3Client.ListBucketsAsync(cancellationToken);

        return response.Buckets.Select(b => b.BucketName!).ToList();
    }

    public async Task<string> StartMultipartUpload(
        string fileName,
        string contentType,
        FileLocation location,
        CancellationToken cancellationToken)
    {
        await CreateBucketIfNotExists(location.BucketName, cancellationToken);

        var initiateRequest = new InitiateMultipartUploadRequest
        {
            BucketName = location.BucketName, Key = location.FileId, ContentType = contentType,
        };

        // TODO: с русскими символами не пропускает
        initiateRequest.Metadata.Add("file-name", fileName);
        initiateRequest.Metadata.Add("file-extension", Path.GetExtension(fileName));

        var result = await _s3Client.InitiateMultipartUploadAsync(initiateRequest, cancellationToken);

        return result.UploadId;
    }

    public async Task<string> GenerateChunkUploadUrl(
        FileLocation location,
        string uploadId,
        int partNumber)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = location.BucketName,
            Key = location.FileId,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(60),
            PartNumber = partNumber,
            UploadId = uploadId,
            Protocol = Protocol.HTTPS,
        };

        return await _s3Client.GetPreSignedURLAsync(request);
    }

    public async Task<string> GenerateUploadUrl(string fileName, FileLocation location, CancellationToken cancellationToken)
    {
        await CreateBucketIfNotExists(location.BucketName, cancellationToken);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = location.BucketName,
            Key = location.FileId,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(60),
            Protocol = Protocol.HTTPS,
        };

        request.Metadata.Add("file-name", fileName);

        return await _s3Client.GetPreSignedURLAsync(request);
    }

    public async Task<string> CompleteMultipartUploadAsync(
        FileLocation location,
        string uploadId,
        List<(int PartNumber, string ETag)> partETags,
        CancellationToken cancellationToken)
    {
        var completeRequest = new CompleteMultipartUploadRequest
        {
            BucketName = location.BucketName,
            Key = location.FileId,
            UploadId = uploadId,
            PartETags = partETags.Select(pt => new PartETag(pt.PartNumber, pt.ETag)).ToList(),
        };

        var response = await _s3Client.CompleteMultipartUploadAsync(completeRequest, cancellationToken);

        return response.Key;
    }

    public async Task AbortMultipartUploadAsync(
        FileLocation fileLocation,
        string uploadId,
        CancellationToken cancellationToken)
    {
        var abortRequest = new AbortMultipartUploadRequest
        {
            BucketName = fileLocation.BucketName, Key = fileLocation.FileId, UploadId = uploadId,
        };

        await _s3Client.AbortMultipartUploadAsync(abortRequest, cancellationToken);
    }

    public async Task<ListMultipartUploadsResponse> ListMultipartUploadAsync(
        string bucketName,
        CancellationToken cancellationToken)
    {
        var listRequest = new ListMultipartUploadsRequest
        {
            BucketName = bucketName,
        };

        var response = await _s3Client.ListMultipartUploadsAsync(listRequest, cancellationToken);

        return response;
    }

    public async Task<string> GenerateDownloadUrlAsync(FileLocation location, int expirationHours)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = location.BucketName,
            Key = location.FileId,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddHours(expirationHours),
            Protocol = Protocol.HTTPS,
        };

        return await _s3Client.GetPreSignedURLAsync(request);
    }

    public async Task<IReadOnlyList<FileUrl?>> GenerateDownloadUrlsAsync(
        IEnumerable<FileLocation> locations,
        int expirationHours)
    {
        var semaphore = new SemaphoreSlim(50);

        var tasks = locations.Select(async location =>
        {
            await semaphore.WaitAsync();
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = location.BucketName,
                    Key = location.FileId,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.UtcNow.AddHours(expirationHours),
                    Protocol = Protocol.HTTPS,
                };

                string? url = await _s3Client.GetPreSignedURLAsync(request);

                return new FileUrl(location.FileId, url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error with generating download url for file {fileId}", location.FileId);

                return null;
            }
            finally
            {
                semaphore.Release();
            }
        });

        return await Task.WhenAll(tasks);
    }

    public async Task<string> DownloadFileAsync(FileLocation location, string tempInputPath, CancellationToken cancellationToken)
    {
        var request = new GetObjectRequest
        {
            BucketName = location.BucketName, Key = location.FileId,
        };

        var response = await _s3Client.GetObjectAsync(request, cancellationToken);

        string filePath = location.FileId + response.Metadata["file-extension"];

        string fileDirectory = Path.Combine(tempInputPath, filePath);

        await using var fileStream = File.Create(fileDirectory);
        await response.ResponseStream.CopyToAsync(fileStream, cancellationToken);

        return fileDirectory;
    }

    public async Task UploadFileAsync(FileLocation location, string? contentType, Stream file, CancellationToken cancellationToken)
    {
        await CreateBucketIfNotExists(location.BucketName, cancellationToken);

        var request = new PutObjectRequest
        {
            BucketName = location.BucketName, Key = location.FileId, InputStream = file,
        };

        if (!string.IsNullOrWhiteSpace(contentType))
            request.ContentType = contentType;

        await _s3Client.PutObjectAsync(request, cancellationToken);
    }

    private async Task CreateBucketIfNotExists(string bucketName, CancellationToken cancellationToken)
    {
        var response = await _s3Client.ListBucketsAsync(cancellationToken);
        if (response.Buckets.Any(b => b.BucketName.Equals(bucketName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var bucketRequest = new PutBucketRequest
        {
            BucketName = bucketName, UseClientRegion = true,
        };

        await _s3Client.PutBucketAsync(bucketRequest, cancellationToken);
    }
}