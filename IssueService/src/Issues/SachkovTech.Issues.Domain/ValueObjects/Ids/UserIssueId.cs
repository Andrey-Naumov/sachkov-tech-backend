using CSharpFunctionalExtensions;

namespace SachkovTech.Issues.Domain.ValueObjects.Ids;

public class UserIssueId : ComparableValueObject
{
    private UserIssueId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; init; }

    public static UserIssueId NewUserIssueId() => new(Guid.NewGuid());

    public static UserIssueId Empty() => new(Guid.Empty);

    public static UserIssueId Create(Guid id) => new(id);

    public static implicit operator UserIssueId(Guid id) => new(id);

    public static implicit operator Guid(UserIssueId userIssueId)
    {
        ArgumentNullException.ThrowIfNull(userIssueId);
        return userIssueId.Value;
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return Value;
    }
}