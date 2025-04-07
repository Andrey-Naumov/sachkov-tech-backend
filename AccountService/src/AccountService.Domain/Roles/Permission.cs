namespace AccountService.Domain.Roles;

public class Permission
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public ICollection<Role> Roles { get; set; } = [];
}