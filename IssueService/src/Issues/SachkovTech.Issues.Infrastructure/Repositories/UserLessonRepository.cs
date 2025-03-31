using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.LessonsViewing;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SachkovTech.Issues.Infrastructure.DbContexts;
using SharedKernel;

namespace SachkovTech.Issues.Infrastructure.Repositories;

public class UserLessonRepository : IUserLessonRepository
{
    private readonly IssuesDbContext _dbContext;

    public UserLessonRepository(IssuesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Add(UserLesson userLesson, CancellationToken cancellationToken)
    {
        await _dbContext.UserLessons.AddAsync(userLesson, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return userLesson.Id;
    }

    public async Task<Result<UserLesson, Error>> GetUserLesson(
        UserId userId,
        LessonId lessonId,
        CancellationToken cancellationToken)
    {
        var userIssue = await _dbContext.UserLessons
            .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.LessonId == lessonId, cancellationToken);

        if (userIssue is null)
            return Errors.General.NotFound();

        return userIssue;
    }
}