using FaqService.Contracts.Messaging;
using FaqService.Contracts.Messaging.Events;
using FaqService.Contracts.Requests;
using FaqService.Entities.ValueObjects;
using FaqService.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FaqService.Features.Question;

public class CreateQuestion
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("question", Handler);
        }
    }

    private static async Task<IResult> Handler(
        [FromBody] CreateQuestionRequest request,
        [FromServices] UnitOfWork unitOfWork,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] IPublishEndpoint publishEndpoint,
        [FromServices] ILogger<CreateQuestion> logger,
        CancellationToken cancellationToken)
    {
        using var transaction = await unitOfWork.BeginTransaction(cancellationToken);
        var id = Guid.NewGuid();

        var pullRequestLinkResult = PullRequestLink.Create(request.ReplLink);

        if (pullRequestLinkResult.IsSuccess == false)
            return ResultResponse.BadRequest(pullRequestLinkResult.Error);

        var questionResult = Entities.Question.Create(
            id,
            request.Title,
            request.Description,
            pullRequestLinkResult.Value,
            request.UserId,
            request.IssueId,
            request.LessonId,
            request.Tags);

        if (questionResult.IsFailure)
            return ResultResponse.BadRequest(questionResult.Error);

        await dbContext.Questions.AddAsync(questionResult.Value, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        QuestionUpdatedEvent questionUpdatedEvent = new(
            questionResult.Value.Id,
            questionResult.Value.Title,
            questionResult.Value.Description,
            questionResult.Value.PullRequestLink.Value,
            questionResult.Value.Status,
            questionResult.Value.CreatedAt,
            questionResult.Value.Tags,
            questionResult.Value.IssueId,
            questionResult.Value.LessonId);

        transaction.Commit();

        await publishEndpoint.Publish(questionUpdatedEvent, cancellationToken);

        logger.LogInformation("Created question {QuestionId}", questionResult.Value);

        return ResultResponse.Ok(questionResult.Value.Id);
    }
}