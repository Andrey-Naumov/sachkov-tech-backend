using AccountService.Application.Interfaces;
using AccountService.Contracts.Messaging;
using AccountService.Domain.Users;
using CSharpFunctionalExtensions;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SharedKernel;

namespace AccountService.Application.Commands.Register;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IRolesRepository _rolesRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IValidator<RegisterUserCommand> _validator;

    public RegisterUserHandler(
        UserManager<User> userManager,
        IUnitOfWork unitOfWork,
        ILogger<RegisterUserHandler> logger,
        IPublishEndpoint publishEndpoint,
        IValidator<RegisterUserCommand> validator,
        IRolesRepository rolesRepository)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _rolesRepository = rolesRepository;
    }

    public async Task<UnitResult<ErrorList>> Handle(
        RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToList();

        using var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        var role = await _rolesRepository
            .GetRoleByName(ParticipantAccount.PARTICIPANT, cancellationToken);
        if (role is null)
            return Errors.General.NotFound(null, "Роль").ToErrorList();

        var userResult = User.CreateParticipant(command.UserName, command.Email, role);
        if (userResult.IsFailure)
            return userResult.Error.ToErrorList();

        var user = userResult.Value;

        var persistUserResult = await _userManager.CreateAsync(user, command.Password);
        if (!persistUserResult.Succeeded)
        {
            var errors = persistUserResult.Errors.Select(e => Error.Failure(e.Code, e.Description));
            return new ErrorList(errors);
        }

        await _unitOfWork.SaveChanges(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        await _publishEndpoint.Publish(new UserRegisteredEvent(user.Id), cancellationToken);

        var sanitizedUserName =
            command.UserName.Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
        _logger.LogInformation("User was created with name {userName}", sanitizedUserName);

        return Result.Success<ErrorList>();
    }
}