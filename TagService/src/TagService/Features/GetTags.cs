using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Core.Database;
using SachkovTech.Framework.Endpoints;
using SharedKernel;
using TagService.API;
using TagService.Entities;
using TagService.Infrastructure;

namespace TagService.Features;

public class GetTags
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/tags/{cursor}", Handler)
                .RequireAuthorization(Permissions.Tags.READ_TAG);
        }

        public async Task<IResult> Handler(
            [FromQuery] string? cursor,
            [FromQuery] int limit,
            [FromServices] ApplicationDbContext context,
            [FromServices] ILogger<DeleteTag> logger,
            CancellationToken cancellationToken)
        {
            if (limit < 1)
                return ResultResponse.BadRequest<ErrorList>(Errors.General.ValueIsInvalid());

            var query = context.Tags.AsNoTracking().AsQueryable();
            
            var result = await query.ToCursorList<Tag, DateTime, Guid>(
                cursor,
                limit,
                x => x.CreatedAt,
                cancellationToken);
            
            if(result.IsFailure)
                return ResultResponse.BadRequest<ErrorList>(result.Error); 

            logger.LogInformation("Fetching tags with cursor: {CursorId}, limit: {Limit}", cursor, limit);

            return ResultResponse.Ok(result.Value);
        }
    }
}