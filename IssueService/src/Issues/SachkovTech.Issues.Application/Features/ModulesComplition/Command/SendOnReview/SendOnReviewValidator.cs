using FluentValidation;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Domain.ValueObjects;

namespace SachkovTech.Issues.Application.Features.ModulesComplition.Command.SendOnReview;

public class SendOnReviewValidator : AbstractValidator<SendOnReviewCommand>
{
    public SendOnReviewValidator()
    {
        RuleFor(s => s.PullRequestUrl).MustBeValueObject(PullRequestUrl.Create);
    }
}