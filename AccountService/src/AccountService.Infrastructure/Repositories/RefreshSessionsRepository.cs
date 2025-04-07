using AccountService.Application.Interfaces;
using AccountService.Domain.RefreshTokens;
using AccountService.Infrastructure.DbContexts;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AccountService.Infrastructure.Repositories;

public class RefreshSessionsRepository(AccountsDbContext accountsContext) : IRefreshSessionsRepository
{
    public async Task<Result<RefreshSession, Error>> GetByRefreshToken(
        Guid refreshToken, CancellationToken cancellationToken)
    {
        var refreshSession = await accountsContext.RefreshSessions
            .Include(r => r.User)
            .ThenInclude(u => u.Roles)
            .FirstOrDefaultAsync(r => r.RefreshToken == refreshToken, cancellationToken);

        if (refreshSession is null)
            return Errors.General.NotFound(refreshToken);

        return refreshSession;
    }

    public void Delete(RefreshSession refreshSession)
    {
        accountsContext.RefreshSessions.Remove(refreshSession);
    }
}