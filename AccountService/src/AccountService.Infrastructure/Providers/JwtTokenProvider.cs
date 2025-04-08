using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AccountService.Application.Interfaces;
using AccountService.Application.Models;
using AccountService.Application.Providers;
using AccountService.Domain.RefreshTokens;
using AccountService.Domain.Users;
using AccountService.Infrastructure.DbContexts;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SachkovTech.Core.RsaKeys;
using SachkovTech.Framework.Authorization;
using SharedKernel;

namespace AccountService.Infrastructure.Providers;

public class JwtTokenProvider : ITokenProvider
{
    private readonly IRolesRepository _rolesRepository;
    private readonly AccountsDbContext _accountContext;
    private readonly IRsaKeyProvider _rsaKeyProvider;
    private readonly AuthOptions _authOptions;

    public JwtTokenProvider(
        IOptions<AuthOptions> options,
        IRolesRepository rolesRepository,
        AccountsDbContext accountContext,
        IRsaKeyProvider rsaKeyProvider)
    {
        _rolesRepository = rolesRepository;
        _accountContext = accountContext;
        _rsaKeyProvider = rsaKeyProvider;
        _authOptions = options.Value;
    }

    public async Task<JwtTokenResult> GenerateAccessToken(User user, CancellationToken cancellationToken)
    {
        var rsaKey = _rsaKeyProvider.GetPrivateRsa();

        var key = new RsaSecurityKey(rsaKey);

        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var roleClaims = user.Roles.Select(r => new Claim(CustomClaims.ROLE, r.Name ?? string.Empty));

        var permissions = await _rolesRepository.GetPermissionCodesByUserId(user.Id, cancellationToken);
        var permissionClaims = permissions.Select(p => new Claim(CustomClaims.PERMISSION, p));

        Claim[] claims =
        [
            new(CustomClaims.ID, user.Id.ToString()),
        ];

        claims = claims
            .Concat(roleClaims)
            .Concat(permissionClaims)
            .ToArray();

        var jwtToken = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_authOptions.ExpiredMinutesTime)),
            signingCredentials: signingCredentials,
            claims: claims);

        string? jwtStringToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        return new JwtTokenResult(jwtStringToken);
    }

    public async Task<Guid> GenerateRefreshToken(User user, CancellationToken cancellationToken)
    {
        var refreshSession = new RefreshSession
        {
            User = user,
            ExpiresIn = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            RefreshToken = Guid.NewGuid(),
        };

        _accountContext.Add(refreshSession);
        await _accountContext.SaveChangesAsync(cancellationToken);

        return refreshSession.RefreshToken;
    }

    public async Task<Result<IReadOnlyList<Claim>, Error>> GetUserClaims(
        string jwtToken, CancellationToken cancellationToken)
    {
        var rsaKey = _rsaKeyProvider.GetPrivateRsa();

        var key = new RsaSecurityKey(rsaKey);

        var jwtHandler = new JwtSecurityTokenHandler();

        var validationParameters = TokenValidationParametersFactory.CreateWithoutLifeTime(key);

        var validationResult = await jwtHandler.ValidateTokenAsync(jwtToken, validationParameters);
        if (validationResult.IsValid == false)
        {
            return Errors.Auth.InvalidToken();
        }

        return validationResult.ClaimsIdentity.Claims.ToList();
    }
}