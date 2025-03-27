using FaqService.Contracts.Requests;
using FaqService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FaqService.Features.Answer;

public class UpdateAnswerMainInfo
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("answers/{answerId:guid}/main-info", Handler);
        }
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid answerId,
        [FromBody] UpdateAnswerMainInfoRequest request,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] ILogger<UpdateAnswerMainInfo> logger,
        CancellationToken cancellationToken)
    {
        var answer = await dbContext.Answers.SingleOrDefaultAsync(p => p.Id == answerId, cancellationToken);
        if (answer is null)
            return ResultResponse.NotFound(Errors.General.NotFound(answerId));

        var result = answer.UpdateMainInfo(request.Text);
        if (result.IsFailure)
            return ResultResponse.NotFound(Errors.General.NotFound(answerId));

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated answer main info {answerId}", answerId);

        return ResultResponse.Ok(answerId);
    }
}