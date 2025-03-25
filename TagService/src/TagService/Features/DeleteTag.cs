using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;
using TagService.Infrastructure;
using Permissions = TagService.API.Permissions;

namespace TagService.Features;

public class DeleteTag
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/tags/{tagId:guid}", Handler)
                .RequireAuthorization(Permissions.Tags.DELETE_TAG);
        }

        public async Task<IResult> Handler(
            [FromRoute] Guid tagId,
            [FromServices] ApplicationDbContext context,
            [FromServices] ILogger<DeleteTag> logger,
            CancellationToken cancellationToken)
        {
            var tag = await context.Tags
                .FirstOrDefaultAsync(t => t.Id == tagId, cancellationToken);

            if (tag is null)
                return ResultResponse.BadRequest<ErrorList>(Errors.General.NotFound());

            context.Tags.Remove(tag);

            await context.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Tag deleted");

            return ResultResponse.Ok();
        }
    }
}