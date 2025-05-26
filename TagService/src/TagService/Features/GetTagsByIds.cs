using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using TagService.Infrastructure;

namespace TagService.Features;

public class GetTagsByIds
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/tags/ids", Handler)
                .RequirePermissions(Permissions.Tags.READ_TAG);
        }
        
        public async Task<IResult> Handler(
            [FromBody] IEnumerable<Guid> ids,
            [FromServices] ApplicationDbContext context,
            [FromServices] ILogger<DeleteTag> logger,
            CancellationToken cancellationToken)
        {
            var tagsQuery = context.Tags
                .AsNoTracking()
                .Where(t => ids.Contains(t.Id));
            
            var result = await tagsQuery.ToListAsync(cancellationToken);
            
            logger.LogInformation("Fetching {Count} tags by IDs: {Ids}", ids.Count(), string.Join(", ", ids ?? []));

            return ResultResponse.Ok(result); 
        }
    }
}