using CommentService.Api;
using CommentService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace CommentService.Features;

public class UpdateRatingDecreaseComment
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/comments/{id:guid}/rating-decrease", Handle)
                .RequirePermissions(Permissions.Comments.UPDATE_COMMENT);
        }

        public async Task<IResult> Handle(
            [FromRoute] Guid id,
            ApplicationDbContext dbContext,
            ILogger<UpdateRatingDecreaseComment> logger,
            CancellationToken cancellationToken)
        {
            var comment = await dbContext.Comments.FirstOrDefaultAsync(
                c => c.Id == id,
                cancellationToken);

            if (comment is null)
                return ResultResponse.NotFound(Errors.General.NotFound(id));

            comment.RatingDecrease();

            dbContext.Comments.Attach(comment);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Changed rating-decrease of comment with {id}", id);
            return ResultResponse.Ok(id);
        }
    }
}
