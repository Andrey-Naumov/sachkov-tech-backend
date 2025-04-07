using FluentValidation;
using SachkovTech.Core.Validation;
using SharedKernel;

namespace SachkovTech.Issues.Application.Features.IssuesComplition.Commands.CompleteIssue;

public class CompleteIssueCommandValidator : AbstractValidator<CompleteIssueCommand>
{
    public CompleteIssueCommandValidator()
    {
        RuleFor(c => c.UserIssueId)
            .NotEmpty().WithError(Errors.General.ValueIsInvalid("id"));
    }
}