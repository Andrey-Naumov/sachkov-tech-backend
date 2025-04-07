using AccountService.Domain.Users;

namespace AccountService.Domain.RefreshTokens;

public class RefreshSession
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public User User { get; init; } = default!;

    public Guid RefreshToken { get; init; }

    public DateTime ExpiresIn { get; init; }

    public DateTime CreatedAt { get; init; }
}