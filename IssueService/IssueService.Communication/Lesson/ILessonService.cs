using CSharpFunctionalExtensions;
using SachkovTech.Issues.Contracts.Lesson;
using SharedKernel;

namespace IssueService.Communication.Lesson;

public interface ILessonService
{
    Task<Result<LessonDto, ErrorList>> GetLessonById(Guid lessonId, CancellationToken cancellationToken);
}