using FileService.Contracts;
using FileService.FilesManagement;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FileService.Features;

public static class GetChunkUploadUrl
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/files/multipart/url", Handler)
                .RequirePermissions(Permissions.Files.READ_FILES);
        }
    }

    private static async Task<IResult> Handler(
        GetChunkUploadUrlRequest request,
        IS3Provider s3Provider,
        CancellationToken cancellationToken)
    {
        if (request.PartNumber <= 0)
        {
            return ResultResponse.BadRequest<GetChunkUploadUrlResponse>(
                Errors.General.ValueIsInvalid("PartNumber должен быть положительным числом."));
        }

        string uploadUrl = await s3Provider.GenerateChunkUploadUrl(
            new FileLocation(request.FileId, request.BucketName),
            request.UploadId,
            request.PartNumber);

        return ResultResponse.Ok(new GetChunkUploadUrlResponse(
            uploadUrl,
            request.PartNumber));
    }
}