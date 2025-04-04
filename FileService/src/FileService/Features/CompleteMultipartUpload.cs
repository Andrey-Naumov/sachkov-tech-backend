using FileService.Contracts;
using FileService.FilesManagement;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FileService.Features;

public static class CompleteMultipartUpload
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/files/multipart/end", Handler)
                .RequirePermissions(Permissions.Files.UPLOAD_FILES);
        }
    }

    private static async Task<IResult> Handler(
        CompleteMultipartUploadRequest request,
        IS3Provider s3Provider,
        CancellationToken cancellationToken)
    {
        if (request.PartETags.Count == 0)
        {
            return ResultResponse.BadRequest<CompleteMultipartUploadResponse>(
                Errors.General.ValueIsInvalid("PartETags должен содержать хотя бы одну часть."));
        }

        var partETags = request.PartETags
            .Select(p => (p.PartNumber, p.ETag))
            .ToList();

        string key = await s3Provider.CompleteMultipartUploadAsync(
            new FileLocation(request.FileId, request.BucketName),
            request.UploadId,
            partETags,
            cancellationToken);

        return ResultResponse.Ok(new CompleteMultipartUploadResponse(key));
    }
}