using FaqService.Contracts.Messaging;
using FaqService.Contracts.Messaging.Events;
using FaqService.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;
using SharedKernel;

namespace FaqService.Features.Question;

public class DeleteQuestion
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("questions/{id:guid}", Handler);
        }
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid id,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] IPublishEndpoint publishEndpoint,
        [FromServices] ILogger<DeleteQuestion> logger,
        CancellationToken cancellationToken)
    {
        var question = await dbContext.Questions.Include(q => q.Answers).SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (question is null)
            return ResultResponse.BadRequest(Errors.General.NotFound(id));

        dbContext.Questions.Remove(question);

        await dbContext.SaveChangesAsync(cancellationToken);

        await publishEndpoint.Publish(new QuestionDeletedEvent(question.Id), cancellationToken);

        logger.LogInformation("Question {id} was deleted.", question.Id);

        return ResultResponse.Ok(id);
    }
}