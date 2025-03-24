using AccountService.Application.Managers;
using AccountService.Contracts.Messaging;
using AccountService.Domain;
using CSharpFunctionalExtensions;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SharedKernel;

namespace AccountService.Application.Commands.Register;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IAccountsManager _accountsManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IValidator<RegisterUserCommand> _validator;

    public RegisterUserHandler(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IAccountsManager accountsManager,
        IUnitOfWork unitOfWork,
        ILogger<RegisterUserHandler> logger,
        IPublishEndpoint publishEndpoint,
        IValidator<RegisterUserCommand> validator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _accountsManager = accountsManager;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToList();

        var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        try
        {
            var role = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.Name == ParticipantAccount.PARTICIPANT, cancellationToken);

            if (role is null)
                return Errors.General.NotFound(null, "role").ToErrorList();

            var userResult = User.CreateParticipant(command.UserName, command.Email, role);

            if (userResult.IsFailure)
                return userResult.Error.ToErrorList();

            var user = userResult.Value;

            var result = await _userManager.CreateAsync(user, command.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => Error.Failure(e.Code, e.Description));
                return new ErrorList(errors);
            }

            var participantAccount = new ParticipantAccount(user);

            await _accountsManager.CreateParticipantAccount(participantAccount, cancellationToken);

            await _unitOfWork.SaveChanges(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            await _publishEndpoint.Publish(new UserRegisteredEvent(user.Id), cancellationToken);

            var sanitizedUserName =
                command.UserName.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");
            _logger.LogInformation("User was created with name {userName}", sanitizedUserName);

            return Result.Success<ErrorList>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User registration was failed");

            await transaction.RollbackAsync(cancellationToken);

            return Error.Failure("register.user", "User registration was failed").ToErrorList();
        }
    }
}