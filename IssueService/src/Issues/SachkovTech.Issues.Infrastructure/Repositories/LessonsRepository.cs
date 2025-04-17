using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Lesson;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SachkovTech.Issues.Infrastructure.DbContexts;
using SharedKernel;

namespace SachkovTech.Issues.Infrastructure.Repositories;

public class LessonsRepository : ILessonsRepository
{
    private readonly IssuesDbContext _dbContext;

    public LessonsRepository(IssuesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Add(Lesson lesson, CancellationToken cancellationToken = default)
    {
        await _dbContext.Lessons.AddAsync(lesson, cancellationToken);
        return lesson.Id;
    }

    public async Task<Result<Lesson, Error>> GetById(
        LessonId lessonId, CancellationToken cancellationToken = default)
    {
        var module = await _dbContext.Lessons
            .FirstOrDefaultAsync(m => m.Id == lessonId, cancellationToken);

        if (module is null)
            return Errors.General.NotFound(lessonId);

        return module;
    }

    public async Task<Result<Lesson, Error>> GetByTitle(
        Title title, CancellationToken cancellationToken = default)
    {
        var module = await _dbContext.Lessons
            .FirstOrDefaultAsync(m => m.Title == title, cancellationToken);

        if (module is null)
            return Errors.General.NotFound();

        return module;
    }

    public async Task<Result<IReadOnlyList<Lesson>, Error>> GetLessonsByTagId(
        Guid tagId, CancellationToken cancellationToken = default)
    {
        var lessons = await _dbContext.Lessons
            .Where(l => l.Tags.Any(t => t == tagId))
            .ToListAsync(cancellationToken);

        return lessons;
    }
}