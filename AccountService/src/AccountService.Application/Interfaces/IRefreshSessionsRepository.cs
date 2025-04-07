using AccountService.Domain.RefreshTokens;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace AccountService.Application.Interfaces;

public interface IRefreshSessionsRepository
{
    Task<Result<RefreshSession, Error>> GetByRefreshToken(
        Guid refreshToken, CancellationToken cancellationToken);

    void Delete(RefreshSession refreshSession);
}