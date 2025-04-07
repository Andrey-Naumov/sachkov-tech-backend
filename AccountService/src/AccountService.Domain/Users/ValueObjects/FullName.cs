using CSharpFunctionalExtensions;
using SharedKernel;

namespace AccountService.Domain.Users.ValueObjects;

public class FullName : ComparableValueObject
{
    public static readonly FullName Empty = new FullName(null, null, null);

    private FullName(string? firstName, string? secondName, string? thirdName)
    {
        FirstName = firstName;
        SecondName = secondName;
        ThirdName = thirdName;
    }

    public string? FirstName { get; }

    public string? SecondName { get; }

    public string? ThirdName { get; }

    public static Result<FullName, Error> Create(string? firstName, string? secondName, string? thirdName)
    {
        return new FullName(firstName, secondName, thirdName);
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return FirstName ?? string.Empty;
        yield return SecondName ?? string.Empty;
        yield return ThirdName ?? string.Empty;
    }
}