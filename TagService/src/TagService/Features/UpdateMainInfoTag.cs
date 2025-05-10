using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;
using TagService.Contracts.Requests;
using TagService.Infrastructure;
using IResult = Microsoft.AspNetCore.Http.IResult;
using Permissions = TagService.Permissions;

namespace TagService.Features;

public class UpdateMainInfoTag
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/tags/{tagId:guid}", Handler)
                .RequireAuthorization(Permissions.Tags.UPDATE_TAG);
        }

        public async Task<IResult> Handler(
            [FromRoute] Guid tagId,
            UpdateMainInfoTagRequest request,
            [FromServices] ApplicationDbContext context,
            [FromServices] ILogger<DeleteTag> logger,
            CancellationToken cancellationToken)
        {
            var tag = await context.Tags
                .FirstOrDefaultAsync(t => t.Id == tagId, cancellationToken);
        
            if(tag is null)
                return ResultResponse.BadRequest<ErrorList>(Errors.General.NotFound());
        
            var result = tag.Edit(request.Name, request.Description);
            if(result.IsFailure)
                return ResultResponse.BadRequest<ErrorList>(result.Error);
        
            context.Tags.Attach(tag);
            await context.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Tag updated with {TagId}", tag.Id);
        
            return ResultResponse.Ok(tag.Id);
        }
    }
}