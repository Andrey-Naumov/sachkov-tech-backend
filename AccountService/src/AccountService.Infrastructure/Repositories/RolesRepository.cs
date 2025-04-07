using AccountService.Application.Interfaces;
using AccountService.Domain.Roles;
using AccountService.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Repositories;

public class RolesRepository(AccountsDbContext accountsContext) : IRolesRepository
{
    public async Task<Permission?> GetPermissionByCode(string code)
        => await accountsContext.Permissions.FirstOrDefaultAsync(p => p.Code == code);

    public async Task<IEnumerable<Permission>?> GetAllPermissions(CancellationToken cancellationToken = default)
        => await accountsContext.Permissions.ToListAsync(cancellationToken);

    public async Task<IEnumerable<string>> GetAllExistingPermissionsCodes(
        CancellationToken cancellationToken = default)
        => await accountsContext.Permissions.Select(p => p.Code).ToListAsync();

    public async Task AddRange(
        IEnumerable<Permission> permissions, CancellationToken cancellationToken = default)
    {
        await accountsContext.Permissions.AddRangeAsync(permissions, cancellationToken);
    }

    public async Task<HashSet<string>> GetPermissionCodesByUserId(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var perms = await accountsContext.ReadUsers
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .SelectMany(r => r.Permissions)
            .Select(p => p.Code)
            .ToHashSetAsync(cancellationToken);

        return perms;
    }

    public async Task AddRolesWithPermissions(
        IEnumerable<Role> rolesWithPermissions, CancellationToken cancellationToken = default)
    {
        await accountsContext.Roles.AddRangeAsync(rolesWithPermissions, cancellationToken);
    }

    public async Task ClearRolesAndPermissions(CancellationToken cancellationToken = default)
    {
        await accountsContext.Roles.ExecuteDeleteAsync(cancellationToken);
        await accountsContext.Permissions.ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<Role?> GetRoleByName(string name, CancellationToken cancellationToken = default)
    {
        return await accountsContext.Roles.FirstOrDefaultAsync(r => r.Name == name);
    }
}