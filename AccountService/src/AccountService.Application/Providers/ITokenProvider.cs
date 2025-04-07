using System.Security.Claims;
using AccountService.Application.Models;
using AccountService.Domain.Users;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace AccountService.Application.Providers;

public interface ITokenProvider
{
    Task<JwtTokenResult> GenerateAccessToken(User user, CancellationToken cancellationToken);

    Task<Guid> GenerateRefreshToken(User user, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<Claim>, Error>> GetUserClaims(
        string jwtToken, CancellationToken cancellationToken);
}