using AccountService.Application.Interfaces;
using AccountService.Domain.Users;
using AccountService.Infrastructure.DbContexts;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Errors = SharedKernel.Errors;

namespace AccountService.Infrastructure.Repository;

public class UserRepository : IUserRepository
{
    private readonly AccountsDbContext _dbContext;

    public UserRepository(AccountsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<User, Error>> GetById(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return Errors.General.NotFound(userId, nameof(userId));

        return user;
    }

    public async Task<User?> GetByPhoneNumber(
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(p => p.PhoneNumber == phoneNumber, cancellationToken);

        return user;
    }

    public async Task<bool> UserNameExists(
        string userName,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AnyAsync(u => u.UserName == userName, cancellationToken);
    }
}