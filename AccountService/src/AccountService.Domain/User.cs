using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace AccountService.Domain;

public class User : IdentityUser<Guid>
{
    private List<Role> _roles = [];
    private List<SocialNetwork> _socialNetworks = [];

    // ef core
    private User()
    {
    }

    public DateTime RegistrationDate { get; set; }

    public FullName FullName { get; set; } = null!;

    public Avatar Avatar { get; set; } = Avatar.None;

    public IReadOnlyList<Role> Roles => _roles;

    public IReadOnlyList<SocialNetwork> SocialNetworks => _socialNetworks;

    public StudentAccount? StudentAccount { get; private set; }

    public SupportAccount? SupportAccount { get; private set; }

    public AdminAccount? AdminAccount { get; private set; }

    public static Result<User, Error> CreateAdmin(
        string userName,
        string email,
        FullName fullName,
        Role role)
    {
        if (role.Name != AdminAccount.ADMIN)
            return Errors.Auth.InvalidRole();

        return new User
        {
            UserName = userName,
            Email = email,
            RegistrationDate = DateTime.UtcNow,
            FullName = fullName,
            _roles = [role],
            _socialNetworks = [],
        };
    }

    public static Result<User, Error> CreateParticipant(
        string userName,
        string email,
        Role role)
    {
        if (role.Name != ParticipantAccount.PARTICIPANT)
            return Errors.Auth.InvalidRole();

        return new User
        {
            UserName = userName,
            Email = email,
            RegistrationDate = DateTime.UtcNow,
            FullName = FullName.Empty,
            _roles = [role],
            _socialNetworks = [],
        };
    }

    public void EnrollParticipant(Role role)
    {
        if (!_roles.Contains(role) && role.Name == StudentAccount.STUDENT)
            _roles.Add(role);
    }

    public UnitResult<ErrorList> UpdateProfile(string userName, FullName fullName, IEnumerable<SocialNetwork> socials)
    {
        var socialsList = socials.ToList();

        if (socialsList.Count > 5)
        {
            return Errors.General.ValueIsInvalid("Социальные сети").ToErrorList();
        }

        UserName = userName;
        FullName = fullName;
        _socialNetworks = socialsList;

        return UnitResult.Success<ErrorList>();
    }

    public void UpdateEmail(string email)
    {
        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
    }

    public void UpdatePhoneNumber(string? phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

    public void UpdateAvatar(Avatar avatar)
    {
        Avatar = avatar;
    }

    public void UpdateSocialNetworks(IEnumerable<SocialNetwork> socialNetworks) =>
        _socialNetworks = socialNetworks.ToList();
}