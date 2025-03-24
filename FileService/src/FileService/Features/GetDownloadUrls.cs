using FileService.Contracts;
using FileService.FilesManagement;
using SachkovTech.Framework.Authorization;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FileService.Features;

public static class GetDownloadUrls
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/files/urls", Handler)
                .RequireAuthorization(Permissions.Files.READ_FILES);
        }
    }

    private static async Task<IResult> Handler(
        GetDownloadUrlsRequest request,
        IS3Provider s3Provider,
        CancellationToken cancellationToken)
    {
        if (!request.Locations.Any())
        {
            return ResultResponse.BadRequest<GetChunkUploadUrlResponse>(Errors.General.ValueIsInvalid("FileIds"));
        }

        var downloadUrls = await s3Provider.GenerateDownloadUrlsAsync(
            request.Locations,
            24);

        return ResultResponse.Ok(new GetDownloadUrlsResponse(downloadUrls));
    }
}