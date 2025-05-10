using CommentService.Api;
using CommentService.Contracts.Requests;
using CommentService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace CommentService.Features;

public class UpdateMainInfoComment
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/comments/{id:guid}/main-info", Handle)
                .RequirePermissions(Permissions.Comments.UPDATE_COMMENT);
        }

        public async Task<IResult> Handle(
            [FromRoute] Guid id,
            [FromBody] UpdateMainInfoCommentRequest request,
            ApplicationDbContext dbContext,
            ILogger<UpdateMainInfoComment> logger,
            CancellationToken cancellationToken)
        {
            var comment = await dbContext.Comments.FirstOrDefaultAsync(
                c => c.Id == id,
                cancellationToken);

            if (comment is null)
                return ResultResponse.NotFound(Errors.General.NotFound(id));

            var result = comment.Edit(request.Text);

            if (result.IsFailure)
                return ResultResponse.BadRequest(result.Error);

            dbContext.Comments.Attach(comment);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Changed comment with {id}", id);
            return ResultResponse.Ok();
        }
    }
}
