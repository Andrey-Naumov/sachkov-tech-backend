using AccountService.Domain.Roles;

namespace AccountService.Application.Interfaces;

public interface IRolesRepository
{
    Task<Permission?> GetPermissionByCode(string code);

    Task<IReadOnlyList<Permission>> GetAllPermissions(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetAllExistingPermissionsCodes(CancellationToken cancellationToken = default);

    Task AddRange(IEnumerable<Permission> permissions, CancellationToken cancellationToken = default);

    Task<HashSet<string>> GetPermissionCodesByUserId(Guid userId, CancellationToken cancellationToken = default);

    Task AddRolesWithPermissions(IEnumerable<Role> rolesWithPermissions, CancellationToken cancellationToken = default);

    Task ClearRolesAndPermissions(CancellationToken cancellationToken = default);

    Task<Role?> GetRoleByName(string name, CancellationToken cancellationToken = default);
}