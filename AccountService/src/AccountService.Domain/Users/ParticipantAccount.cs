namespace AccountService.Domain.Users;

public class ParticipantAccount
{
    public const string PARTICIPANT = "Participant";

    public ParticipantAccount(User user)
    {
        User = user;
    }

    // ef core ctor
    private ParticipantAccount()
    {
    }

    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; }
}