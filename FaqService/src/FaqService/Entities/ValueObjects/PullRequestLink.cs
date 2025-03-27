using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using SharedKernel;
using static FaqService.Constants.Constants;

namespace FaqService.Entities.ValueObjects;

public partial class PullRequestLink : ComparableValueObject
{
    private PullRequestLink(string link)
    {
        Value = link;
    }

    public static Result<PullRequestLink, Error> Create(string link)
    {
        if (PullRequestUrlRegex().Match(link).Success == false)
            return Errors.General.ValueIsInvalid(nameof(link));

        return new PullRequestLink(link);
    }

    public string Value { get; } = null!;

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(LINK_PATTERN)]
    private static partial Regex PullRequestUrlRegex();
}
