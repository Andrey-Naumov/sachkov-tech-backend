using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Core.Database;
using SachkovTech.Framework.Endpoints;
using SachkovTech.Issues.Contracts.Issue;
using SharedKernel;
using TagService.Entities;
using TagService.Infrastructure;
using YamlDotNet.Core;
using Permissions = TagService.API.Permissions;

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

            var queryTags = context.Tags.AsNoTracking().AsQueryable();
            var queryCount = context.Tags.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(cursor))
            {
                var decodedCursor = Cursor<DateTime>.Decode(cursor);
                if (decodedCursor is null)
                    return ResultResponse.BadRequest<ErrorList>(Errors.General.ValueIsInvalid());

                queryTags = queryTags.Where(x => EF.Functions.LessThanOrEqual(
                    ValueTuple.Create(x.CreatedAt, x.Id),
                    ValueTuple.Create(decodedCursor.Filter, decodedCursor.LastId)));

                queryCount = queryTags.Where(x => EF.Functions.LessThanOrEqual(
                    ValueTuple.Create(x.CreatedAt, x.Id),
                    ValueTuple.Create(decodedCursor.Filter, decodedCursor.LastId)
                ));
            }

            var items = await queryTags
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .Take(limit + 1)
                .ToListAsync(cancellationToken);
            
            var count = await queryCount
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .CountAsync(cancellationToken);

            var hasMore = limit < count;

            string? newCursor = null;
            if (hasMore && items.Count > 0)
            {
                var lastItem = items[^1];

                items.Remove(lastItem);

                newCursor = Cursor<DateTime>.Encode(lastItem.CreatedAt, lastItem.Id);
            }

            var result = new CursorList<Tag>(items, newCursor, hasMore);

            logger.LogInformation("Fetching tags with cursor: {CursorId}, limit: {Limit}", cursor, limit);

            return ResultResponse.Ok(result);
        }
    }
}