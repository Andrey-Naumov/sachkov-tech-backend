using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SharedKernel;

namespace SachkovTech.Issues.Domain.LessonsViewing;

public class UserLesson : DomainEntity<UserLessonId>
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
        IsCompleted = true;
    }

    public LessonId LessonId { get; private set; }

    public Guid UserId { get; private set; }

    public bool IsCompleted { get; private set; }

    public void CompleteWatching() => IsCompleted = true;

    public void CancelWatching() => IsCompleted = false;
}