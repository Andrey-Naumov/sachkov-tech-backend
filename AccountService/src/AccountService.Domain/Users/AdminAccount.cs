namespace AccountService.Domain.Users;

public class AdminAccount
{
    public const string ADMIN = "Admin";

    public AdminAccount(User user)
    {
        User = user;
    }

    // ef core ctor
    private AdminAccount()
    {
    }

    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
}