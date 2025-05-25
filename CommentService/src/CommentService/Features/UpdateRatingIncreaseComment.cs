using CommentService.Api;
using CommentService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace CommentService.Features;

public class UpdateRatingIncreaseComment
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/comments{id:guid}/rating-increase", Handle)
                .RequirePermissions(Permissions.Comments.CREATE_COMMENT);
        }

        public async Task<IResult> Handle(
            [FromRoute] Guid id,
            ApplicationDbContext dbContext,
            ILogger<UpdateRatingIncreaseComment> logger,
            CancellationToken cancellationToken)
        {
            var comment = await dbContext.Comments.FirstOrDefaultAsync(
                c => c.Id == id,
                cancellationToken);

            if (comment is null)
                return ResultResponse.NotFound(GeneralErrors.NotFound(id));

            comment.RatingIncrease();

            dbContext.Comments.Attach(comment);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Changed rating-increase of comment with {id}", id);
            return ResultResponse.Ok();
        }
    }
}
