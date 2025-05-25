using CommentService.Contracts.Requests;
using CommentService.Entities;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using SachkovTech.Core.Database;
using SachkovTech.Framework.Endpoints;

namespace CommentService.Features;

public class GetChildrenComments
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/comments/receiving-children", Handle);
        }

        public async Task<IResult> Handle(
            [FromBody] GetChildrenCommentsRequest request,
            ISqlConnectionFactory connectionFactory)
        {
            using var connection = connectionFactory.Create();

            var cursor = Cursor<DateTime, Guid>.Decode(request.Cursor);

            var sqlPage = """
                          SELECT
                          id           AS Id,
                          parent_id    AS ParentId,
                          relation_id  AS RelationId,
                          user_id      AS UserId,
                          text,
                          created_at   AS CreatedAt,
                          rating
                          FROM comments.comments
                          WHERE parent_id = @ParentId
                          """
                          + (cursor is not null
                              ? """

                                AND (created_at < @CreatedAt
                                  OR (created_at = @CreatedAt AND id < @LastId))
                                """
                              : "")
                          + """

                            ORDER BY created_at DESC, id DESC
                            LIMIT @Limit;
                            """;

            var pageAll = (await connection.QueryAsync<CommentDto>(sqlPage, new
            {
                ParentId = request.ParentId, CreatedAt = cursor?.Filter, LastId = cursor?.LastId, Limit = request.Limit + 1
            })).ToList();

            string? nextCursor = null;
            if (pageAll.Count > request.Limit)
            {
                var extra = pageAll[request.Limit];
                nextCursor = Cursor<DateTime, Guid>.Encode(extra.CreatedAt, extra.Id);
                pageAll.RemoveAt(request.Limit);
            }

            if (pageAll.Count == 0)
                return Results.Ok(new
                {
                    children = Array.Empty<CommentDto>(), nextCursor
                });

            var ids = pageAll.Select(x => x.Id).ToArray();

            var sqlSub = """
                         SELECT
                           id           AS Id,
                           parent_id    AS ParentId,
                           relation_id  AS RelationId,
                           user_id      AS UserId,
                           text,
                           created_at   AS CreatedAt,
                           rating
                         FROM (
                           SELECT *,
                                  ROW_NUMBER() OVER(
                                    PARTITION BY parent_id
                                    ORDER BY created_at DESC, id DESC
                                  ) AS rn
                           FROM comments.comments
                           WHERE parent_id = ANY(@Ids)
                         ) t
                         WHERE t.rn <= @SubLimit
                         ORDER BY parent_id, created_at DESC, id DESC;
                         """;

            var subs = (await connection.QueryAsync<CommentDto>(sqlSub, new
            {
                Ids = ids, SubLimit = 2
            })).ToList();

            var map = subs
                .GroupBy(c => c.ParentId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var child in pageAll)
            {
                if (!map.TryGetValue(child.Id, out var list))
                    continue;

                child.Children = list;
                if (list.Count != 2)
                    continue;

                child.HasMoreChildren = true;
                var last = list[^1];
                child.ChildrenCursor = Cursor<DateTime, Guid>.Encode(last.CreatedAt, last.Id);
            }

            return Results.Ok(new
            {
                children = pageAll, nextCursor
            });
        }
    }
}