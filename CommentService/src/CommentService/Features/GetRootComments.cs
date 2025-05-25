using CommentService.Contracts.Requests;
using CommentService.Entities;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using SachkovTech.Core.Database;
using SachkovTech.Framework.Endpoints;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace CommentService.Features;

public class GetRootComments
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/comments/roots", Handle);
        }

        public async Task<IResult> Handle(
            [FromBody] GetRootCommentsRequest request,
            ISqlConnectionFactory connectionFactory)
        {
            using var connection = connectionFactory.Create();

            var cursor = Cursor<DateTime, Guid>.Decode(request.Cursor);

            string sqlRoots = """
                              SELECT
                                id           AS Id,
                                parent_id    AS ParentId,
                                relation_id  AS RelationId,
                                user_id      AS UserId,
                                text,
                                created_at   AS CreatedAt,
                                rating
                              FROM comments.comments
                              WHERE relation_id = @RelationId
                                AND parent_id   IS NULL
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

            var rootsParams = new
            {
                RelationId = request.RelationId, Limit = request.Limit + 1, CreatedAt = cursor?.Filter, LastId = cursor?.LastId
            };

            var roots = (await connection.QueryAsync<CommentDto>(sqlRoots, rootsParams)).ToList();

            string? nextCursor = null;
            if (roots.Count > request.Limit)
            {
                var extra = roots[request.Limit];
                nextCursor = Cursor<DateTime, Guid>.Encode(extra.CreatedAt, extra.Id);
                roots.RemoveAt(request.Limit);
            }

            if (roots.Count == 0)
                return Results.Ok(new GetRootCommentsResponse([], null));

            var parentIds = roots.Select(r => r.Id).ToArray();

            string sqlChildren = """
                                 SELECT
                                   id        AS Id,
                                   parent_id AS ParentId,
                                   relation_id AS RelationId,
                                   user_id   AS UserId,
                                   text as Text,
                                   created_at AS CreatedAt,
                                   rating as Rating
                                 FROM (
                                   SELECT *,
                                          ROW_NUMBER() OVER(
                                            PARTITION BY parent_id
                                            ORDER BY created_at DESC, id DESC
                                          ) AS rn
                                   FROM comments.comments
                                   WHERE parent_id = ANY(@ParentIds)
                                 ) t
                                 WHERE t.rn <= @ChildLimit
                                 ORDER BY parent_id, created_at DESC, id DESC;
                                 """;

            var children = (await connection.QueryAsync<CommentDto>(
                sqlChildren,
                new
                {
                    ParentIds = parentIds, ChildLimit = 2
                }
            )).ToList();

            var childrenMap = children
                .GroupBy(c => c.ParentId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var root in roots)
            {
                if (!childrenMap.TryGetValue(root.Id, out var list))
                    continue;

                root.Children = list;
                if (list.Count != 2)
                    continue;

                root.HasMoreChildren = true;
                var last = list[^1];
                root.ChildrenCursor = Cursor<DateTime, Guid>.Encode(last.CreatedAt, last.Id);
            }

            return Results.Ok(new GetRootCommentsResponse(roots, nextCursor));
        }
    }
}