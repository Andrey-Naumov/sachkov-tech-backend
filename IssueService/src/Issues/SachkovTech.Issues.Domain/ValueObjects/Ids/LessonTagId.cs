using CSharpFunctionalExtensions;

namespace SachkovTech.Issues.Domain.ValueObjects.Ids;

public class LessonTagId : ComparableValueObject
{
    private LessonTagId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static LessonTagId NewLessonTagId() => new LessonTagId(Guid.NewGuid());

    public static LessonTagId Create(Guid id) => new(id);

    public static implicit operator LessonTagId(Guid id) => new(id);

    public static implicit operator Guid(LessonTagId lessonTagId)
    {
        ArgumentNullException.ThrowIfNull(lessonTagId);
        return lessonTagId.Value;
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return Value;
    }
}