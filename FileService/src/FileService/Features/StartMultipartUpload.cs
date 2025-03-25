using FileService.Contracts;
using FileService.FilesManagement;
using SachkovTech.Framework.Endpoints;

namespace FileService.Features;

public static class StartMultipartUpload
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/files/multipart/start", Handler)
                .RequireAuthorization(Permissions.Files.UPLOAD_FILES);
        }
    }

    private static async Task<IResult> Handler(
        StartMultipartUploadRequest request, IS3Provider s3Provider,
        CancellationToken cancellationToken)
    {
        string fileId = Guid.NewGuid().ToString();

        (long chunkSize, int totalChunks) = ChunkSizeCalculator.Calculate(request.FileSize);

        string uploadId = await s3Provider.StartMultipartUpload(
            request.FileName,
            request.ContentType,
            new FileLocation(fileId, request.BucketName),
            cancellationToken);

        return ResultResponse.Ok(new StartMultipartUploadResponse(
            fileId,
            uploadId,
            request.BucketName,
            chunkSize,
            totalChunks));
    }
}