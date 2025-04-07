using AccountService.Application.Interfaces;
using AccountService.Application.Providers;
using AccountService.Contracts.Responses;
using CSharpFunctionalExtensions;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Database;
using SharedKernel;

namespace AccountService.Application.Commands.RefreshTokens;

public class RefreshTokensHandler : ICommandHandler<LoginResponse, RefreshTokensCommand>
{
    private readonly IRefreshSessionsRepository _refreshSessionManager;
    private readonly ITokenProvider _tokenProvider;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokensHandler(
        IRefreshSessionsRepository refreshSessionManager,
        ITokenProvider tokenProvider,
        IUnitOfWork unitOfWork)
    {
        _refreshSessionManager = refreshSessionManager;
        _tokenProvider = tokenProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponse, ErrorList>> Handle(
        RefreshTokensCommand command,
        CancellationToken cancellationToken = default)
    {
        var oldRefreshSession = await _refreshSessionManager
            .GetByRefreshToken(command.RefreshToken, cancellationToken);

        if (oldRefreshSession.IsFailure)
            return oldRefreshSession.Error.ToErrorList();

        if (oldRefreshSession.Value.ExpiresIn < DateTime.UtcNow)
        {
            return Errors.Auth.ExpiredToken().ToErrorList();
        }

        _refreshSessionManager.Delete(oldRefreshSession.Value);
        await _unitOfWork.SaveChanges(cancellationToken);

        var accessToken = await _tokenProvider
            .GenerateAccessToken(oldRefreshSession.Value.User, cancellationToken);

        var refreshToken = await _tokenProvider
            .GenerateRefreshToken(oldRefreshSession.Value.User, cancellationToken);

        var roles = oldRefreshSession.Value.User.Roles
            .Where(r => !string.IsNullOrEmpty(r.Name))
            .Select(r => r.Name!.ToLower());

        return new LoginResponse(
            accessToken.AccessToken,
            refreshToken,
            oldRefreshSession.Value.User.Id,
            roles);
    }
}