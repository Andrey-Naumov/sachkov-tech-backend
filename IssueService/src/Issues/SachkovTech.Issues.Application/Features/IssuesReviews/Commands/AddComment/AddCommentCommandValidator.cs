using FluentValidation;
using SachkovTech.Core.Validation;
using SachkovTech.Issues.Domain.IssuesReviews.ValueObjects;

namespace SachkovTech.Issues.Application.Features.IssuesReviews.Commands.AddComment;

public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(c => c.Message)
            .MustBeValueObject(Message.Create);
    }
}