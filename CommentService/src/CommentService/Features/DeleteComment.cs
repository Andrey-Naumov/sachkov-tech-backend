using CommentService.Api;
using CommentService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace CommentService.Features;

public class DeleteComment
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("api/comments/{id:guid}", Handle)
                .RequirePermissions(Permissions.Comments.DELETE_COMMENT);
        }

        public async Task<IResult> Handle(
            [FromRoute] Guid id,
            ApplicationDbContext dbContext,
            ILogger<DeleteComment> logger,
            CancellationToken cancellationToken)
        {
            var comment = await dbContext.Comments.FirstOrDefaultAsync(
                c => c.Id == id,
                cancellationToken);

            if (comment is null)
                return ResultResponse.NotFound(Errors.General.NotFound(id));

            var parentComment = await dbContext.Comments
                    .FirstOrDefaultAsync(a => a.Id == comment.ParentId);

            if (parentComment != null)
                parentComment.RepliesCountDecrease();

            dbContext.Comments.Remove(comment);

            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Deleted comment with {id}", id);
            return ResultResponse.Ok();
        }
    }
}
