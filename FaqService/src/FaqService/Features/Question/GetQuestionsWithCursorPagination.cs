using FaqService.Contracts.Requests;
using FaqService.Dtos;
using FaqService.Extensions;
using FaqService.Infrastructure;
using FaqService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Framework.Endpoints;

namespace FaqService.Features.Question;

public class GetQuestionsWithCursorPagination
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("questions", Handler);
        }
    }

    private static async Task<IResult> Handler(
        [AsParameters] GetQuestionsQuery query,
        [FromServices] SearchRepository searchRepository,
        [FromServices] ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var questionIds = await searchRepository.SearchQuestions(query, cancellationToken);

        var questionsQuery = dbContext.Questions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id));

        var paginatedQuestionsDtos = await QuestionsToDtoCursorListWithOrderedIds(
            source: questionsQuery,
            cursor: query.Cursor,
            orderedIds: questionIds,
            limit: query.Limit,
            cancellationToken: cancellationToken);

        return ResultResponse.Ok(paginatedQuestionsDtos);
    }

    private static async Task<CursorList<QuestionDto>> QuestionsToDtoCursorListWithOrderedIds(
        IQueryable<Entities.Question> source,
        List<Guid> orderedIds,
        Guid? cursor,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (orderedIds.Count == 0)
        {
            return new CursorList<QuestionDto>(
                items: [],
                cursor: cursor,
                nextCursor: null,
                limit: limit,
                totalCount: 0);
        }

        int totalCount = await source.CountAsync(cancellationToken);

        int cursorIndex = cursor.HasValue ? orderedIds.IndexOf(cursor.Value) : -1;
        var questionIdsForPagination = cursorIndex >= 0
            ? orderedIds.Skip(cursorIndex + 1).Take(limit).ToList()
            : orderedIds.Take(limit).ToList();

        var itemsFromDb = await source
            .Where(p => questionIdsForPagination.Contains(p.Id))
            .Select(q => new
            {
                Question = q, AnswerCount = q.Answers.Count,
            })
            .OrderBy(x => questionIdsForPagination.IndexOf(x.Question.Id))
            .Take(limit)
            .ToListAsync(cancellationToken);

        Guid? nextCursorId =
            itemsFromDb.Count == limit ? itemsFromDb.Last().Question.Id : null;

        var questionDtos = itemsFromDb.Select(a => new QuestionDto
        {
            Id = a.Question.Id,
            Title = a.Question.Title,
            Description = a.Question.Description,
            PullRequestLink = a.Question.PullRequestLink.Value,
            Status = a.Question.Status,
            CreatedAt = a.Question.CreatedAt,
            Tags = a.Question.Tags,
            IssueId = a.Question.IssueId,
            LessonId = a.Question.LessonId,
            SolutionId = a.Question.Solution?.Id,
            CountOfAnswers = a.AnswerCount,
        }).ToList();

        return new CursorList<QuestionDto>(
            items: questionDtos,
            cursor: cursor,
            nextCursor: nextCursorId,
            limit: limit,
            totalCount: totalCount);
    }
}