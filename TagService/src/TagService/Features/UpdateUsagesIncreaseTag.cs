using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;
using TagService.Infrastructure;
using IResult = Microsoft.AspNetCore.Http.IResult;
using Permissions = TagService.API.Permissions;

namespace TagService.Features;

public class UpdateUsagesIncreaseTag
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/tags/{tagId:guid}/rating-increase", Handler)
                .RequireAuthorization(Permissions.Tags.UPDATE_TAG);
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
        
            tag.UsagesIncrease();

            await context.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Tag increased with {TagId}", tag.Id);

            return ResultResponse.Ok(tag.Id);
        }
    }
}