using FileService.Contracts;
using FileService.FilesManagement;
using SachkovTech.Framework.Authorization;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FileService.Features;

public static class GetHlsPlaylistUrl
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/files/hls/{videoId}/playlist", Handler)
                .RequireAuthorization(Permissions.Files.READ_FILES);
        }
    }

    private static async Task<IResult> Handler(
        string videoId,
        IS3Provider s3Provider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(videoId))
        {
            return ResultResponse.BadRequest<GetHlsPlaylistUrlResponse>(Errors.General.ValueIsInvalid("VideoId обязателен."));
        }

        var playlistLocation = new FileLocation($"{videoId}/master.m3u8", "videos");

        string playlistUrl = await s3Provider.GenerateDownloadUrlAsync(playlistLocation, 24);

        return ResultResponse.Ok(new GetHlsPlaylistUrlResponse(playlistUrl));
    }
}