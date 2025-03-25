using FileService.Contracts;
using FileService.FilesManagement;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FileService.Features;

public static class GetDownloadUrl
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/files/url", Handler)
                .RequireAuthorization(Permissions.Files.READ_FILES);
        }
    }

    private static async Task<IResult> Handler(
        GetDownloadUrlRequest request,
        IS3Provider s3Provider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.FileId))
        {
            return ResultResponse.BadRequest<GetChunkUploadUrlResponse>(Errors.General.ValueIsInvalid("FileId обязателен."));
        }

        string downloadUrl = await s3Provider.GenerateDownloadUrlAsync(new FileLocation(request.FileId, request.BucketName), 24);

        return ResultResponse.Ok(new GetDownloadUrlResponse(downloadUrl));
    }
}