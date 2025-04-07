using AccountService.Application.Database;
using AccountService.Application.Interfaces;
using AccountService.Domain;
using AccountService.Domain.Users;
using AccountService.Domain.Users.ValueObjects;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SharedKernel;

namespace AccountService.Application.Commands.UpdateProfile;

public class UpdateProfileHandler : ICommandHandler<Guid, UpdateProfileCommand>
{
    private readonly IValidator<UpdateProfileCommand> _validator;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateProfileHandler> _logger;

    public UpdateProfileHandler(
        IValidator<UpdateProfileCommand> validator,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateProfileHandler> logger,
        UserManager<User> userManager)
    {
        _validator = validator;
        _userRepository = userRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        UpdateProfileCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        bool userNameAlreadyExists = await _userRepository.UserNameExists(command.Dto.UserName, cancellationToken);
        if (userNameAlreadyExists)
            return UserErrors.UserNameAlreadyExist();

        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
            return Errors.General.NotFound(command.UserId).ToErrorList();

        var fullname = FullName.Create(command.Dto.FirstName, command.Dto.SecondName, command.Dto.ThirdName).Value;

        var socials = command.Dto.Socials
            .Select(s => SocialNetwork.Create(s.Name, s.Link).Value);

        var updateResult = user.UpdateProfile(command.Dto.UserName, fullname, socials);
        if (updateResult.IsFailure)
            return updateResult.Error;

        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Updated user full name successfully for {UserId}.", command.UserId);

        return user.Id;
    }
}