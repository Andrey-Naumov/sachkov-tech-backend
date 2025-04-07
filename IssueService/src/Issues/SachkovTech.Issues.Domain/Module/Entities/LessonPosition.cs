using CSharpFunctionalExtensions;
using SachkovTech.Issues.Domain.Module.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;

namespace SachkovTech.Issues.Domain.Module.Entities;

public class LessonPosition : Entity<LessonPositionId>, IPositionable
{
    public LessonPosition(LessonPositionId id, LessonId lessonId, Position position)
        : base(id)
    {
        LessonId = lessonId;
        Position = position;
    }

    // ef core
    private LessonPosition()
    {
    }

    public LessonId LessonId { get; private set; } = null!;

    public Position Position { get; private set; } = null!;

    public void SetPosition(Position position)
    {
        Position = position;
    }
}