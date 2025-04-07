using Microsoft.AspNetCore.Identity;

namespace AccountService.Domain.Roles;

public sealed class Role : IdentityRole<Guid>
{
    public List<Permission> Permissions { get; set; } = [];
}