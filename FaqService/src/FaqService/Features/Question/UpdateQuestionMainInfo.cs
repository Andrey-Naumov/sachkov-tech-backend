using FaqService.Contracts.Messaging.Commands;
using FaqService.Contracts.Messaging.Events;
using FaqService.Contracts.Requests;
using FaqService.Extensions;
using FaqService.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FaqService.Features.Question;

public class UpdateQuestionMainInfo
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("questions/{questionId:guid}/main-info", Handler);
        }
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid questionId,
        [FromBody] UpdatePostMainInfoRequest request,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] ElasticIndexRecoveryService indexRecoveryService,
        [FromServices] UnitOfWork unitOfWork,
        [FromServices] IPublishEndpoint publishEndpoint,
        [FromServices] ILogger<UpdateQuestionMainInfo> logger,
        CancellationToken cancellationToken)
    {
        using var transaction = await unitOfWork.BeginTransaction(cancellationToken);

        var question = await dbContext.Questions.SingleOrDefaultAsync(p => p.Id == questionId, cancellationToken);
        if (question is null)
            return ResultResponse.BadRequest(Errors.General.NotFound(questionId));

        var result = question.UpdateMainInfo(request.Title, request.Description);
        if (result.IsFailure)
            return ResultResponse.BadRequest(result.Error);

        await dbContext.SaveChangesAsync(cancellationToken);

        QuestionUpdatedEvent updateQuestionEvent = new(
            question.Id,
            question.Title,
            question.Description,
            question.PullRequestLink.Value,
            question.Status,
            question.CreatedAt,
            question.Tags,
            question.IssueId,
            question.LessonId);

        transaction.Commit();

        await publishEndpoint.Publish(updateQuestionEvent, cancellationToken);

        logger.LogInformation("Updated question main info {questionId}", questionId);

        return ResultResponse.Ok(question.Id);
    }
}