using AccountService.Contracts.Requests;
using AccountService.Domain.Users.ValueObjects;
using FluentValidation;
using SachkovTech.Core.Validation;
using SharedKernel;

namespace AccountService.Application.Commands.UpdateProfile;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(u => u.Dto).SetValidator(new UpdateProfileRequestValidator());
    }
}

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(u => u.UserName).NotEmpty();

        RuleFor(u => new
        {
            u.FirstName, u.SecondName, u.ThirdName,
        }).MustBeValueObject(f => FullName.Create(f.FirstName, f.SecondName, f.ThirdName));

        RuleFor(u => u.Socials).Must(s => s.Count() <= 5).WithError(Errors.General.ValueIsInvalid("Социальные сети"));

        RuleForEach(u => u.Socials).MustBeValueObject(s => SocialNetwork.Create(s.Name, s.Link));
    }
}