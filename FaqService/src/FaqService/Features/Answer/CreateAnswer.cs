using FaqService.Contracts.Requests;
using FaqService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FaqService.Features.Answer;

public class CreateAnswer
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("questions/{questionId:guid}/answers", Handler);
        }
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid questionId,
        [FromBody] CreateAnswerRequest request,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] ILogger<CreateAnswer> logger,
        CancellationToken cancellationToken)
    {
        var question = await dbContext.Questions.SingleOrDefaultAsync(p => p.Id == questionId, cancellationToken);
        if (question is null)
            return ResultResponse.NotFound(Errors.General.NotFound(questionId));

        var answerResult = Entities.Answer.Create(
            questionId,
            request.UserId,
            request.Text);

        if (answerResult.IsFailure)
            return ResultResponse.BadRequest(Errors.General.NotFound(questionId));

        await dbContext.Answers.AddAsync(answerResult.Value, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created answer {answerId}", answerResult.Value.Id);

        return ResultResponse.Ok(answerResult.Value.Id);
    }
}