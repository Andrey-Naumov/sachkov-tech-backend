using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.Lesson;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Application.Interfaces;

public interface ILessonsRepository
{
    public Task<Guid> Add(Lesson lesson, CancellationToken cancellationToken = default);

    public Task<Result<Lesson, Error>> GetById(LessonId lessonId, CancellationToken cancellationToken = default);

    public Task<Result<Lesson, Error>> GetByTitle(Title title, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<Lesson>, Error>> GetLessonsByTagId(
        Guid tagId, CancellationToken cancellationToken = default);
}
