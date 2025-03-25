using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;
using TagService.Entities;
using TagService.Extensions;
using TagService.Infrastructure;
using Permissions = TagService.API.Permissions;

namespace TagService.Features;

public class GetTagById
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

            if (!string.IsNullOrWhiteSpace(cursor))
            {
                var decodedCursor = Cursor.Decode(cursor);
                if (decodedCursor is null)
                    return ResultResponse.BadRequest<ErrorList>(Errors.General.ValueIsInvalid());

                query = query.Where(x => EF.Functions.LessThanOrEqual(
                    ValueTuple.Create(x.CreatedAt, x.Id),
                    ValueTuple.Create(decodedCursor.Date, decodedCursor.LastId)));
            }

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .Take(limit + 1)
                .ToListAsync(cancellationToken);

            var hasMore = items.Count > limit;

            DateTime? nextDate = hasMore ? items[^1].CreatedAt : null;
            Guid? nextId = items.Count > limit ? items[^1].Id : null;

            items.RemoveAt(items.Count - 1);

            var newCursor = nextDate is not null && nextId is not null
                ? Cursor.Encode(nextDate.Value, nextId.Value)
                : null;

            var result = new CursorList<Tag>(items, newCursor, hasMore);

            logger.LogInformation("Fetching tags with cursor: {CursorId}, limit: {Limit}", cursor, limit);

            return ResultResponse.Ok(result);
        }
    }
}