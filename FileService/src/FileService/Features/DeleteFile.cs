using FileService.Contracts;
using FileService.FilesManagement;
using Microsoft.AspNetCore.Mvc;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FileService.Features;

public class DeleteFile
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/files/{id:guid}/delete", Handler)
                .RequirePermissions(Permissions.Files.DELETE_FILES);
        }
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid id,
        [FromQuery] string bucketName,
        [FromServices] IS3Provider s3Provider,
        CancellationToken cancellationToken)
    {
        var fileLocation = new FileLocation(id.ToString(), bucketName);

        var deletedId = await s3Provider.DeleteFileAsync(fileLocation, cancellationToken);
        if (string.IsNullOrWhiteSpace(deletedId))
            return ResultResponse.NotFound<string>(Errors.General.NotFound().ToErrorList());

        return ResultResponse.Ok(deletedId);
    }
}