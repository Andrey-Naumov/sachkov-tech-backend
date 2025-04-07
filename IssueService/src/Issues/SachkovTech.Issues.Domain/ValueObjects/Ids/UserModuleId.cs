using CSharpFunctionalExtensions;

namespace SachkovTech.Issues.Domain.ValueObjects.Ids;

public class UserModuleId : ComparableValueObject
{
    public static readonly UserModuleId Empty = new UserModuleId(Guid.Empty);

    private UserModuleId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static UserModuleId NewUserModuleId() => new(Guid.NewGuid());

    public static UserModuleId Create(Guid id) => new(id);

    public static implicit operator UserModuleId(Guid id) => new(id);

    public static implicit operator Guid(UserModuleId userModuleId)
    {
        ArgumentNullException.ThrowIfNull(userModuleId);
        return userModuleId.Value;
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return Value;
    }
}