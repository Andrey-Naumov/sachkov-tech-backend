using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.LessonsComplition;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Interfaces;

public interface IUserLessonRepository
{
    Task<Guid> Add(UserLesson userLesson, CancellationToken cancellationToken);

    Task<Result<UserLesson, Error>> GetUserLesson(
        UserId userId,
        LessonId lessonId,
        CancellationToken cancellationToken);
}