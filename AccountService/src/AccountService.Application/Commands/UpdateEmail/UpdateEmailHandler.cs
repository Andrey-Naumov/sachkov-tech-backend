using AccountService.Domain;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SachkovTech.Core.Validation;
using SharedKernel;

namespace AccountService.Application.Commands.UpdateEmail;

public class UpdateEmailHandler : ICommandHandler<Guid, UpdateEmailCommand>
{
    private readonly IValidator<UpdateEmailCommand> _validator;
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateEmailHandler> _logger;


    // private readonly ICacheService _cache;
    public UpdateEmailHandler(
            IValidator<UpdateEmailCommand> validator,
            UserManager<User> userManager,
            IUnitOfWork unitOfWork,
            ILogger<UpdateEmailHandler> logger)

        // ICacheService cache
    {
        _validator = validator;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;

        // _cache = cache;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        UpdateEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (validationResult.IsValid == false)
            return validationResult.ToList();

        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
            return Errors.General.NotFound(command.UserId).ToErrorList();

        user.UpdateEmail(command.Email);

        // var key = "users_" + userResult.Value.Id;
        //
        // var userCache = await _cache.GetAsync<UserDataModel>(key, cancellationToken);
        // if (userCache is not null)
        // {
        //     await _cache.RemoveAsync(key, cancellationToken);
        // }
        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Updated user main info successfully for {UserId}.", command.UserId);

        return user.Id;
    }
}