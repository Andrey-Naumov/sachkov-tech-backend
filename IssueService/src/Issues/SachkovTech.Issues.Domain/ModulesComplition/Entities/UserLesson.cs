using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.Domain.ModulesComplition.Entities;

public class UserLesson : Entity<UserLessonId>
{
    // ef core
    private UserLesson(UserLessonId id)
        : base(id)
    {
    }

    public UserLesson(
        UserLessonId id,
        LessonId lessonId,
        Guid userId)
        : base(id)
    {
        LessonId = lessonId;
        UserId = userId;
        StartWatching();
    }

    public LessonId LessonId { get; private set; }

    public Guid UserId { get; private set; }

    public bool IsCompleted { get; private set; }

    internal void StartWatching() => IsCompleted = false;

    internal void CompleteWatching() => IsCompleted = true;

    internal void CancelWatching() => IsCompleted = false;
}