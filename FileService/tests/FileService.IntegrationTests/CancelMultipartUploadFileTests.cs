using System.Net.Http.Json;
using FileService.Contracts;
using FileService.FilesManagement;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace FileService.IntegrationTests;

public class CancelMultipartUploadFileTests : FileServiceTestsBase
{
    private readonly IS3Provider _s3Provider;

    public CancelMultipartUploadFileTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        var scope = factory.Services.CreateScope();
        _s3Provider = scope.ServiceProvider.GetRequiredService<IS3Provider>();
    }

    [Fact]
    public async Task CancelMultipartUpload_AfterStart_Success()
    {
        // Arrange
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", "test.mp4"));
        var cancellationToken = new CancellationTokenSource().Token;

        // Act

        // 1. Start Multipart Upload
        var startResponse = await StartMultipartUpload(fileInfo, cancellationToken);

        // 2. Cancel Multipart Upload
        await CancelMultipartUpload(startResponse, cancellationToken);

        var listMultipartUploads = await _s3Provider.ListMultipartUploadAsync("videos", cancellationToken);

        // Assert
        foreach (var upload in listMultipartUploads.MultipartUploads)
        {
            upload.UploadId.Should().NotBeEquivalentTo(startResponse.UploadId);
        }
    }

    [Fact]
    public async Task CancelMultipartUpload_HalfCycle_Success()
    {
        // Arrange
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", "test.mp4"));
        var cancellationToken = new CancellationTokenSource().Token;

        // Act

        // 1. Start Multipart Upload
        var startResponse = await StartMultipartUpload(fileInfo, cancellationToken);

        var parts = new List<PartETagDto>();
        int partNumber = 1;

        await using var stream = fileInfo.OpenRead();

        // 2. Upload File Parts
        for (int i = 0; i < startResponse.TotalChunksCount / 2; i++)
        {
            byte[] chunk = new byte[startResponse.ChunkSize];
            int bytesRead = await stream.ReadAsync(chunk, 0, (int)startResponse.ChunkSize, cancellationToken);

            if (bytesRead == 0)
                break;

            var chunkUrlResponse = await GenerateChunkUploadUrl(startResponse, partNumber, cancellationToken);

            var eTag = await UploadFilePartToMinio(
                chunkUrlResponse.UploadUrl,
                chunk,
                cancellationToken);

            parts.Add(new PartETagDto(partNumber, eTag!));
            partNumber++;
        }

        // 3. Cancel Multipart Upload
        await CancelMultipartUpload(startResponse, cancellationToken);

        var listMultipartUploads = await _s3Provider.ListMultipartUploadAsync("videos", cancellationToken);

        // Assert
        foreach (var upload in listMultipartUploads.MultipartUploads)
        {
            upload.UploadId.Should().NotBeEquivalentTo(startResponse.UploadId);
        }
    }

    [Fact]
    public async Task CancelMultipartUpload_FullCycle_Success()
    {
        // Arrange
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", "test.mp4"));
        var cancellationToken = new CancellationTokenSource().Token;

        // Act

        // 1. Start Multipart Upload
        var startResponse = await StartMultipartUpload(fileInfo, cancellationToken);

        var parts = new List<PartETagDto>();
        int partNumber = 1;

        await using var stream = fileInfo.OpenRead();

        // 2. Upload File Parts
        for (int i = 0; i < startResponse.TotalChunksCount; i++)
        {
            byte[] chunk = new byte[startResponse.ChunkSize];
            int bytesRead = await stream.ReadAsync(chunk, 0, (int)startResponse.ChunkSize, cancellationToken);

            if (bytesRead == 0)
                break;

            var chunkUrlResponse = await GenerateChunkUploadUrl(startResponse, partNumber, cancellationToken);

            var eTag = await UploadFilePartToMinio(
                chunkUrlResponse.UploadUrl,
                chunk,
                cancellationToken);

            parts.Add(new PartETagDto(partNumber, eTag!));
            partNumber++;
        }

        // 3. Cancel Multipart Upload
        await CancelMultipartUpload(startResponse, cancellationToken);

        var listMultipartUploads = await _s3Provider.ListMultipartUploadAsync("videos", cancellationToken);

        // Assert
        foreach (var upload in listMultipartUploads.MultipartUploads)
        {
            upload.UploadId.Should().NotBeEquivalentTo(startResponse.UploadId);
        }
    }

    private async Task<StartMultipartUploadResponse> StartMultipartUpload(
        FileInfo fileInfo,
        CancellationToken cancellationToken)
    {
        var request = new StartMultipartUploadRequest(
            fileInfo.Name,
            "videos",
            "video/mp4",
            fileInfo.Length);

        var response = await AppHttpClient
            .PostAsJsonAsync("api/files/multipart/start", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<Envelope<StartMultipartUploadResponse>>(cancellationToken);

        return result!.Result!;
    }

    private async Task<GetChunkUploadUrlResponse> GenerateChunkUploadUrl(
        StartMultipartUploadResponse startResponse,
        int partNumber,
        CancellationToken cancellationToken)
    {
        var request = new GetChunkUploadUrlRequest(
            startResponse.FileId,
            startResponse.UploadId,
            partNumber,
            startResponse.BucketName);

        var response = await AppHttpClient
            .PostAsJsonAsync("api/files/multipart/url", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<Envelope<GetChunkUploadUrlResponse>>(cancellationToken);

        return result!.Result!;
    }

    private async Task<string> UploadFilePartToMinio(
        string uploadUrl,
        byte[] chunk,
        CancellationToken cancellationToken)
    {
        using var content = new ByteArrayContent(chunk);

        var response = await HttpClient.PutAsync(uploadUrl, content, cancellationToken);

        response.EnsureSuccessStatusCode();

        return response.Headers.ETag?.Tag?.Trim('"')!;
    }

    private async Task CancelMultipartUpload(
        StartMultipartUploadResponse startResponse,
        CancellationToken cancellationToken)
    {
        var request = new CancelMultipartUploadRequest(
            new FileLocation(startResponse.FileId, startResponse.BucketName),
            startResponse.UploadId);

        var response = await AppHttpClient
            .PostAsJsonAsync("api/files/multipart/cancel", request, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}