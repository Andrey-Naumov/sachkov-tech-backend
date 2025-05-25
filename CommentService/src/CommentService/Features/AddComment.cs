using CommentService.Contracts.Requests;
using CommentService.Entities;
using CommentService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace CommentService.Features;

public class AddComment
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/comments", Handle);
        }

        public async Task<IResult> Handle(
            [FromBody] AddCommentRequest request,
            ApplicationDbContext dbContext,
            ILogger<AddComment> logger,
            CancellationToken cancellationToken)
        {
            var commentResult = Comment.Create(
                            request.RelationId,
                            request.UserId,
                            request.ParentId,
                            request.Text);

            if (commentResult.IsFailure)
                return ResultResponse.BadRequest<Comment>(commentResult.Error);

            await dbContext.Comments.AddAsync(commentResult.Value, cancellationToken);

            if (request.ParentId != null)
            {
                var parentComment = await dbContext.Comments
                    .FirstOrDefaultAsync(a => a.Id == request.ParentId);

                if (parentComment == null)
                    return ResultResponse.NotFound(GeneralErrors.NotFound(request.ParentId, "RealationId"));

                parentComment.RepliesCountIncrease();
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Added comment with {id}", commentResult.Value.Id);
            return ResultResponse.Ok(commentResult.Value.Id);
        }
    }
}
