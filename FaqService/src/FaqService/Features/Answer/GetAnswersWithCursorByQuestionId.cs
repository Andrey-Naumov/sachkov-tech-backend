using FaqService.Contracts.Requests;
using FaqService.Dtos;
using FaqService.Extensions;
using FaqService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;

namespace FaqService.Features.Answer;

public class GetAnswersWithCursorByQuestionId
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("questions/{questionId:guid}/answers-by-cursor", Handler);
        }
    }

    private static async Task<IResult> Handler(
        [FromRoute] Guid questionId,
        [AsParameters] GetAnswersQuery query,
        [FromServices] ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var answersQuery = dbContext.Answers
            .AsNoTracking()
            .Where(p => p.Id == questionId)
            .OrderByDescending(a => a.CreatedAt)
            .AsQueryable();

        var paginatedAnswers = await answersQuery.ToCursorList(
            query.Cursor, query.Limit, cancellationToken);

        var answerDtos = paginatedAnswers.Items.Select(a => new AnswerDto
        {
            Id = a.Id,
            IsSolution = a.IsSolution,
            QuestionId = a.QuestionId,
            Text = a.Text,
            UserId = a.UserId,
            Rating = a.Rating,
            CreatedAt = a.CreatedAt,
        }).ToList();

        return ResultResponse.Ok(new CursorList<AnswerDto>(
            items: answerDtos,
            cursor: paginatedAnswers.Cursor,
            nextCursor: paginatedAnswers.NextCursor,
            limit: paginatedAnswers.Limit,
            totalCount: paginatedAnswers.TotalCount));
    }
}