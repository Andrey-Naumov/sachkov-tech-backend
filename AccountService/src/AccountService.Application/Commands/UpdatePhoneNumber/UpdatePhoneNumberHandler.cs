using AccountService.Application.Database;
using AccountService.Domain;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SharedKernel;

namespace AccountService.Application.Commands.UpdatePhoneNumber;

public class UpdatePhoneNumberHandler : ICommandHandler<Guid, UpdatePhoneNumberCommand>
{
    private readonly IValidator<UpdatePhoneNumberCommand> _validator;
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePhoneNumberHandler> _logger;


    // private readonly ICacheService _cache;
    public UpdatePhoneNumberHandler(
            IValidator<UpdatePhoneNumberCommand> validator,
            UserManager<User> userManager,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdatePhoneNumberHandler> logger)

        // ICacheService cache
    {
        _validator = validator;
        _userManager = userManager;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;

        // _cache = cache;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        UpdatePhoneNumberCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
            return Errors.General.NotFound(command.UserId).ToErrorList();

        if (command.PhoneNumber != null)
        {
            var userPhoneNumberResult = await _userRepository.GetByPhoneNumber(command.PhoneNumber, cancellationToken);

            if (userPhoneNumberResult != null && user.Id != userPhoneNumberResult.Id)
                return Errors.General.AlreadyExist().ToErrorList();
        }

        user.UpdatePhoneNumber(command.PhoneNumber);

        // var key = "users_" + userResult.Value.Id;
        //
        // var userCache = await _cache.GetAsync<UserDataModel>(key, cancellationToken);
        // if (userCache is not null)
        // {
        //     await _cache.RemoveAsync(key, cancellationToken);
        // }
        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Updated user phone number successfully for {UserId}.", command.UserId);

        return user.Id;
    }
}