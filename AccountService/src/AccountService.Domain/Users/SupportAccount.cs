namespace AccountService.Domain.Users;

public class SupportAccount
{
    public const string SUPPORT = "Support";

    public SupportAccount(
        User user,
        string aboutSelf)
    {
        User = user;
        AboutSelf = aboutSelf;
    }

    // ef core ctor
    private SupportAccount()
    {
    }

    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; }

    public string AboutSelf { get; set; }
}