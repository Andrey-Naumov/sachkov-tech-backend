using CommentService.Api;
using CommentService.Contracts.Requests;
using CommentService.Entities;
using CSharpFunctionalExtensions;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using SachkovTech.Core.Database;
using SachkovTech.Framework.Endpoints;
using System.Data;

namespace CommentService.Features;

public class GetChildrenComments
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/comments/receiving-children", Handle)
            .RequirePermissions(Permissions.Comments.READ_COMMENT);
        }

        public async Task<Microsoft.AspNetCore.Http.IResult> Handle(
            [FromBody] GetChildrenCommentsRequest request,
            ISqlConnectionFactory connectionFactory)
        {
            var commentDtos = await GetDataAsync(connectionFactory, request);
            var cursorList = CreateCursorList(commentDtos.ToList());
            return ResultResponse.Ok(cursorList);
        }

        private async Task<IEnumerable<CommentDto>> GetDataAsync(
            ISqlConnectionFactory connectionFactory,
            GetChildrenCommentsRequest request)
        {
            using var connection = connectionFactory.Create();
            var decodedCursor = Cursor<DateTime, Guid>.Decode(request.Cursor);
            var parameters = new DynamicParameters();
            parameters.Add("@ChildrenLimit", Constants.CHILDREN_COUNT_LIMIT);
            parameters.Add("@RelationId", request.RelationId);
            parameters.Add("@ParentId", request.ParentId);

            parameters.Add(
                "@CursorCreatedAt",
                dbType: DbType.DateTime,
                direction: ParameterDirection.Input,
                value: decodedCursor?.Filter);

            parameters.Add(
                "@CursorLastId",
                dbType: DbType.Guid,
                direction: ParameterDirection.Input,
                value: decodedCursor?.LastId);

            var datas = await connection.QueryAsync<CommentDto>(
                """
                WITH children AS(
                     SELECT 
                        ch.id AS Id,
                        ch.relation_id AS RelationId,
                        ch.user_id AS UserId,
                        ch.parent_id AS ParentId,
                        ch.text AS Text,
                        ch.rating AS Rating,
                        ch.created_at AS CreatedAt,
                        ch.replies_count AS RepliesCount,
                        ROW_NUMBER() OVER(PARTITION BY ch.parent_id ORDER BY ch.created_at DESC) AS rn
                     FROM comments.comments ch
                     WHERE ch.relation_id = @RelationId
                        AND ch.parent_id = @ParentId
                        AND
                            CASE 
                                WHEN @CursorCreatedAt IS NOT NULL THEN (ch.created_at, ch.id) 
                                    <= (@CursorCreatedAt, @CursorLastId)
                                ELSE ch.created_at > '01.01.0001 00:00:00'
                            END
                )

                SELECT Id, RelationId, UserId, ParentId, Text, Rating, CreatedAt, RepliesCount
                FROM children
                WHERE rn<@ChildrenLimit + 2
                """
            , parameters);

            return datas;
        }

        public CursorList<CommentDto> CreateCursorList(List<CommentDto> dtos)
        {
            var hasMore = dtos.Count > Constants.CHILDREN_COUNT_LIMIT;
            string? cursor = null;

            if (hasMore)
            {
                cursor = Cursor<DateTime, Guid>.Encode(dtos[^1].CreatedAt, dtos[^1].Id);
                dtos.Remove(dtos.Last());
            }

            var cursorList = new CursorList<CommentDto>(dtos, cursor, hasMore);
            return cursorList;
        }
    }
}
