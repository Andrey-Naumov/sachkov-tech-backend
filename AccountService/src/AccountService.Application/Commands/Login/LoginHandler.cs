using AccountService.Application.Providers;
using AccountService.Contracts.Responses;
using AccountService.Domain.Users;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SachkovTech.Core.Abstractions;
using SharedKernel;

namespace AccountService.Application.Commands.Login;

public class LoginHandler : ICommandHandler<LoginResponse, LoginCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenProvider _tokenProvider;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        UserManager<User> userManager,
        ITokenProvider tokenProvider,
        ILogger<LoginHandler> logger)
    {
        _userManager = userManager;
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    public async Task<Result<LoginResponse, ErrorList>> Handle(
        LoginCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
        {
            return Errors.Auth.InvalidCredentials().ToErrorList();
        }

        var passwordConfirmed = await _userManager.CheckPasswordAsync(user, command.Password);
        if (!passwordConfirmed)
        {
            return Errors.Auth.InvalidCredentials().ToErrorList();
        }

        var accessToken = await _tokenProvider.GenerateAccessToken(user, cancellationToken);
        var refreshToken = await _tokenProvider.GenerateRefreshToken(user, cancellationToken);

        var roles = user.Roles
            .Where(r => !string.IsNullOrEmpty(r.Name))
            .Select(r => r.Name!.ToLower());

        _logger.LogInformation("User {UserId} logged in", user.Id);

        return new LoginResponse(
            accessToken.AccessToken,
            refreshToken,
            user.Id,
            roles);
    }
}