using FaqService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FaqService.Features.Answer.ChangeRating;

public class DecreaseAnswerRating
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("answers/{answerId:guid}/decrease-rating", Handler);
        }
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid answerId,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] ILogger<DecreaseAnswerRating> logger,
        CancellationToken cancellationToken)
    {
        var answer = await dbContext.Answers.SingleOrDefaultAsync(p => p.Id == answerId, cancellationToken);
        if (answer is null)
            return ResultResponse.NotFound(Errors.General.NotFound(answerId));

        answer.DecreaseRating();

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Decreased answer {answerId} rating", answerId);

        return ResultResponse.Ok(answer.Id);
    }
}