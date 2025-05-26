using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;
using TagService.Contracts.Requests;
using TagService.Entities;
using TagService.Infrastructure;

namespace TagService.Features;

public class CreateTag
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/tags", Handler)
                .RequirePermissions(Permissions.Tags.CREATE_TAG);
        }

        public async Task<IResult> Handler(
            AddTagRequest request,
            [FromServices] ApplicationDbContext context,
            [FromServices] ILogger<CreateTag> logger,
            CancellationToken cancellationToken)
        {
            var tagResult = await context.Tags.SingleOrDefaultAsync(t => t.Name == request.Name, cancellationToken);
            if (tagResult is not null)
                return ResultResponse.BadRequest<Tag>(Errors.General.AlreadyExist());

            var tag = Tag.Create(
                request.Name,
                request.Description);

            if (tag.IsFailure)
                return ResultResponse.BadRequest<Tag>(tag.Error);

            await context.Tags.AddAsync(tag.Value, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Added tag with {TagId}", tag.Value.Id);

            return ResultResponse.Ok(tag.Value);
        }
    }
}