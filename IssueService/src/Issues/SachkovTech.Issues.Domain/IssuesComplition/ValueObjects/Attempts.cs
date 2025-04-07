using CSharpFunctionalExtensions;

namespace SachkovTech.Issues.Domain.IssuesComplition.ValueObjects;

public class Attempts : ComparableValueObject
{
    private Attempts(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public Attempts Add() => Create(Value + 1);

    public static Attempts Create(int attempts = 1)
    {
        return new Attempts(attempts);
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return Value;
    }
}