using FaqService.Contracts.Requests;
using FaqService.Extensions;
using FaqService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FaqService.Features.Question;

public class SelectSolution
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("questions/{questionId:guid}/solution", Handler);
        }
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid questionId,
        [FromBody] SelectSolutionRequest request,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] ElasticIndexRecoveryService indexRecoveryService,
        [FromServices] UnitOfWork unitOfWork,
        [FromServices] ILogger<SelectSolution> logger,
        CancellationToken cancellationToken)
    {
        using var transaction = await unitOfWork.BeginTransaction(cancellationToken);

        var question = await dbContext.Questions.SingleOrDefaultAsync(p => p.Id == questionId, cancellationToken);
        if (question is null)
            return ResultResponse.BadRequest(Errors.General.NotFound(questionId));

        var answer = await dbContext.Answers
            .SingleOrDefaultAsync(a => a.Id == request.AnswerId && a.QuestionId == questionId, cancellationToken);
        if (answer is null)
            return ResultResponse.BadRequest(Errors.General.NotFound(request.AnswerId));

        answer.ChangeIsSolution(true);

        await dbContext.SaveChangesAsync(cancellationToken);

        transaction.Commit();

        logger.LogInformation("For question {questionId} selected solution {answerId}", questionId, request.AnswerId);

        return ResultResponse.Ok(question.Id);
    }
}