using FaqService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FaqService.Features.Answer;

public class DeleteAnswer
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("answers/{answerId:guid}", Handler);
        }
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid answerId,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] ILogger<DeleteAnswer> logger,
        CancellationToken cancellationToken)
    {
        var answer = await dbContext.Answers.SingleOrDefaultAsync(a => a.Id == answerId, cancellationToken);
        if (answer is null)
            return ResultResponse.NotFound(Errors.General.NotFound(answerId));

        dbContext.Answers.Remove(answer);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Answer {AnswerId} was deleted", answerId);

        return ResultResponse.Ok(answerId);
    }
}