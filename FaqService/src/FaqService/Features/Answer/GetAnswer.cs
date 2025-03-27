using FaqService.Dtos;
using FaqService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;

namespace FaqService.Features.Answer;

public class GetAnswer
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("answer/{answerId:guid}", Handler);
        }
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid answerId,
        [FromServices] ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var answer = await dbContext.Answers
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == answerId, cancellationToken);

        return ResultResponse.Ok(
            answer is not null
            ? new AnswerDto
            {
                Id = answer.Id,
                IsSolution = answer.IsSolution,
                QuestionId = answer.QuestionId,
                Text = answer.Text,
                UserId = answer.UserId,
                Rating = answer.Rating,
                CreatedAt = answer.CreatedAt,
            }
            : null);
    }
}