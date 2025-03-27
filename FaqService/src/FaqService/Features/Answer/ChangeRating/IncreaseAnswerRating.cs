using FaqService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FaqService.Features.Answer.ChangeRating;

public class IncreaseAnswerRating
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("answers/{answerId:guid}/increase-rating", Handler);
        }
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid answerId,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] ILogger<IncreaseAnswerRating> logger,
        CancellationToken cancellationToken)
    {
        var answer = await dbContext.Answers.SingleOrDefaultAsync(p => p.Id == answerId, cancellationToken);
        if (answer is null)
            return ResultResponse.NotFound(Errors.General.NotFound(answerId));

        answer.IncreaseRating();

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Increased answer {answerId} rating", answerId);

        return ResultResponse.Ok(answer.Id);
    }
}