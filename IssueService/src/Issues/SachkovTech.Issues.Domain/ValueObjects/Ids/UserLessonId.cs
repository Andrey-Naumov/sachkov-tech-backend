using System.Text.Json.Serialization;
using CSharpFunctionalExtensions;

namespace SachkovTech.Issues.Domain.ValueObjects.Ids;

public class UserLessonId : ComparableValueObject
{
    [JsonConstructor]
    private UserLessonId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static UserLessonId NewUserLessonId() => new(Guid.NewGuid());

    public static UserLessonId Create(Guid id) => new(id);

    public static implicit operator UserLessonId(Guid id) => new(id);

    public static implicit operator Guid(UserLessonId userLessonId)
    {
        ArgumentNullException.ThrowIfNull(userLessonId);
        return userLessonId.Value;
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return Value;
    }
}