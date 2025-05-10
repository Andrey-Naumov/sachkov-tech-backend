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

public class GetRootComments
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/comments/receiving-roots", Handle)
            .RequirePermissions(Permissions.Comments.READ_COMMENT);
        }

        public async Task<Microsoft.AspNetCore.Http.IResult> Handle(
            [FromBody] GetRootCommentsRequest request,
            ISqlConnectionFactory connectionFactory)
        {
            var commentDtos = await GetDataAsync(connectionFactory, request);
            var commentsTree = ConvertDtosListToDtosTree(commentDtos);
            var cursorList = CreateCursorList(commentsTree, Constants.PARENTS_COUNT_LIMIT);
            return ResultResponse.Ok(cursorList);
        }

        private async Task<IEnumerable<CommentDto>> GetDataAsync(
            ISqlConnectionFactory connectionFactory,
            GetRootCommentsRequest request)
        {
            using var connection = connectionFactory.Create();
            var decodedCursor = Cursor<DateTime, Guid>.Decode(request.Cursor);
            var parameters = new DynamicParameters();
            parameters.Add("@ParentsLimit", Constants.PARENTS_COUNT_LIMIT);
            parameters.Add("@ChildrenLimit", Constants.CHILDREN_COUNT_LIMIT);
            parameters.Add("@RelationId", request.RelationId);

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
                WITH parents AS(
                     SELECT
                        p.id AS Id,
                        p.relation_id AS RelationId,
                        p.user_id AS UserId,
                        p.parent_id AS ParentId,
                        p.text AS Text,
                        p.rating AS Rating,
                        p.created_at AS CreatedAt,
                        p.replies_count AS RepliesCount,
                        ROW_NUMBER() OVER(PARTITION BY p.parent_id ORDER BY p.created_at DESC) AS rn
                     FROM comments.comments p
                     WHERE p.relation_id = @RelationId
                        AND
                            CASE 
                                WHEN @CursorCreatedAt IS NOT NULL THEN (p.created_at, p.id) 
                                    <= (@CursorCreatedAt, @CursorLastId)
                                ELSE p.created_at > '01.01.0001 00:00:00'
                            END
                        AND p.parent_id IS NULL
                     ORDER BY p.created_at DESC
                     LIMIT @ParentsLimit + 1),

                     children AS(
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
                     WHERE ch.parent_id IN (SELECT id FROM parents) AND ch.parent_id IS NOT NULL)

                SELECT Id, RelationId, UserId, ParentId, Text, Rating, CreatedAt, RepliesCount
                FROM parents
                UNION ALL
                SELECT Id, RelationId, UserId, ParentId, Text, Rating, CreatedAt, RepliesCount
                FROM children
                WHERE rn<@ChildrenLimit + 2
                """
            , parameters);

            return datas;
        }

        private List<CommentDto> ConvertDtosListToDtosTree(IEnumerable<CommentDto> commentDtos)
        {
            Dictionary<Guid, (CommentDto dto, List<CommentDto> dtosList)> pairs = [];

            foreach (var commentDto in commentDtos)
            {
                if (commentDto.ParentId == null)
                {
                    pairs.Add(commentDto.Id, (commentDto, []));
                }
                else if (pairs.TryGetValue(commentDto.ParentId.Value, out var value))
                {
                    value.dtosList.Add(commentDto);
                }
                else
                {
                    pairs.Add(commentDto.Id, (commentDto, []));
                }
            }

            foreach (var commentDto in pairs)
            {
                commentDto.Value.dto.ChildrenComments = CreateCursorList(
                    commentDto.Value.dtosList, Constants.CHILDREN_COUNT_LIMIT);
            }

            return pairs.Values.Select(a => a.dto).ToList();
        }

        private CursorList<CommentDto> CreateCursorList(List<CommentDto> dtos, int limit)
        {
            var hasMore = dtos.Count > limit;
            (DateTime? date, Guid id) cursorData = dtos.Any() ? (dtos[^1].CreatedAt, dtos[^1].Id) : (null, Guid.Empty);

            if (cursorData.date != null && hasMore)
            {
                var encodedCursor = Cursor<DateTime, Guid>.Encode(cursorData.date.Value, cursorData.id);
                dtos.Remove(dtos.Last());
                var cursorList = new CursorList<CommentDto>(dtos, encodedCursor, true);
                return cursorList;
            }

            return new CursorList<CommentDto>(dtos, null, false);
        }
    }
}
