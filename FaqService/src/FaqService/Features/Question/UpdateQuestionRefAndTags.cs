using FaqService.Contracts.Messaging;
using FaqService.Contracts.Messaging.Commands;
using FaqService.Contracts.Messaging.Events;
using FaqService.Contracts.Requests;
using FaqService.Entities.ValueObjects;
using FaqService.Extensions;
using FaqService.Infrastructure;
using FaqService.Infrastructure.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FaqService.Features.Question;

public class UpdateQuestionRefAndTags
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("questions/{questionId:guid}/refs-and-tags", Handler);
        }
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid questionId,
        [FromBody] UpdateQuestionRefAndTagsRequest request,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SearchRepository searchRepository,
        [FromServices] ElasticIndexRecoveryService indexRecoveryService,
        [FromServices] UnitOfWork unitOfWork,
        [FromServices] IPublishEndpoint publishEndpoint,
        [FromServices] ILogger<UpdateQuestionRefAndTags> logger,
        CancellationToken cancellationToken)
    {
        using var transaction = await unitOfWork.BeginTransaction(cancellationToken);

        var question = await dbContext.Questions
            .SingleOrDefaultAsync(p => p.Id == questionId, cancellationToken);
        if (question is null)
            return ResultResponse.BadRequest(Errors.General.NotFound(questionId));

        var pullRequestLinkResult = PullRequestLink.Create(request.ReplLink);
        if (pullRequestLinkResult.IsFailure)
            return ResultResponse.BadRequest(pullRequestLinkResult.Error);

        question.UpdateRefsAndTags(pullRequestLinkResult.Value, request.IssueId, request.LessonId, request.Tags);

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

        logger.LogInformation("Updated refs and tags of question {questionId}", questionId);

        return ResultResponse.Ok(question.Id);
    }
}