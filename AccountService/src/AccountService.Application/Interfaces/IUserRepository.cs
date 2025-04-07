using AccountService.Domain.Users;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace AccountService.Application.Interfaces;

public interface IUserRepository
{
    Task<Result<User, Error>> GetById(Guid userId, CancellationToken cancellationToken = default);

    Task<User?> GetByPhoneNumber(
        string phoneNumber,
        CancellationToken cancellationToken = default);

    Task<bool> UserNameExists(
        string userName,
        CancellationToken cancellationToken = default);
}