using System.Text;
using AccountService.Contracts.Responses;
using AccountService.Domain.Users;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Validation;
using SharedKernel;

namespace AccountService.Application.Commands.GenerateConfirmationLink;

public class GenerateConfirmationLinkHandler
    : ICommandHandler<ConfirmationLinkResponse, GenerateConfirmationLinkCommand>
{
    private const string BASE_URL = "http://localhost:5273/api/Accounts/email-verification/";

    private readonly UserManager<User> _userManager;
    private readonly ILogger<GenerateConfirmationLinkHandler> _logger;

    private readonly IValidator<GenerateConfirmationLinkCommand> _validator;

    public GenerateConfirmationLinkHandler(
        UserManager<User> userManager,
        ILogger<GenerateConfirmationLinkHandler> logger,
        IValidator<GenerateConfirmationLinkCommand> validator)
    {
        _userManager = userManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ConfirmationLinkResponse, ErrorList>> Handle(
        GenerateConfirmationLinkCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToList();

        var user = await _userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            _logger.LogError("Failed to find user with id {UserId}.", command.UserId);

            return Errors.General.NotFound(command.UserId, nameof(command.UserId)).ToErrorList();
        }

        var confirmationLink = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var codeEncoded =
            BASE_URL +
            WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationLink));

        _logger.LogInformation("Generated confirmation link successfully for {UserId}.", command.UserId);

        return new ConfirmationLinkResponse(user.Email!, codeEncoded);
    }
}