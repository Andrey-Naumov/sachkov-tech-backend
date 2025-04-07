namespace AccountService.Infrastructure.Options;

public class RolesPermissionsToSeed
{
    public Dictionary<string, string[]> Permissions { get; set; } = [];

    public Dictionary<string, string[]> Roles { get; set; } = [];
}