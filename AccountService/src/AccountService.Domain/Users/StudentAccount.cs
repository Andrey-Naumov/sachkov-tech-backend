namespace AccountService.Domain.Users;

public class StudentAccount
{
    public const string STUDENT = "Student";

    public StudentAccount(User user)
    {
        Id = Guid.NewGuid();
        User = user;
        DateStartedStudying = DateTime.UtcNow;
    }

    // ef core ctor
    private StudentAccount()
    {
    }

    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; }

    public DateTime DateStartedStudying { get; set; }
}