using System.Text.Json;
using AccountService.Application.Interfaces;
using AccountService.Domain.Roles;
using AccountService.Domain.Users;
using AccountService.Domain.Users.ValueObjects;
using AccountService.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SachkovTech.Core.Database;

namespace AccountService.Infrastructure.Seeding;

public class AccountsSeederService(
    UserManager<User> userManager,
    IRolesRepository rolesPermissionsRepository,
    IOptions<AdminOptions> adminOptions,
    ILogger<AccountsSeederService> logger,
    IUnitOfWork unitOfWork)
{
    private readonly AdminOptions _adminOptions = adminOptions.Value;

    public async Task SeedAsync()
    {
        logger.LogInformation("Seeding accounts...");

        var json = await File.ReadAllTextAsync("etc/accounts.json");

        var seedData = JsonSerializer.Deserialize<RolesPermissionsToSeed>(json)
                       ?? throw new ApplicationException("Could not deserialize roles and permissions to seed.");

        await rolesPermissionsRepository.ClearRolesAndPermissions();

        await SeedPermissions(seedData.Permissions);

        await unitOfWork.SaveChanges();

        await SeedRolesPermissionsRelationship(seedData.Roles);

        await unitOfWork.SaveChanges();

        await SeedAdminAccount();

        await unitOfWork.SaveChanges();
    }

    private async Task SeedPermissions(Dictionary<string, string[]> permissions)
    {
        var permissionEntities = permissions.SelectMany(x => x.Value.Select(y => new Permission { Code = y }));
        await rolesPermissionsRepository.AddRange(permissionEntities);
    }

    private async Task SeedRolesPermissionsRelationship(
        Dictionary<string, string[]> roles, CancellationToken cancellationToken = default)
    {
        var existingPermissions = await rolesPermissionsRepository.GetAllPermissions(cancellationToken);
        if (existingPermissions is null)
            throw new ApplicationException("Could not find permissions in database");

        List<Role> rolesEntities = [];

        foreach (var role in roles)
        {
            Role roleEntity = new() { Name = role.Key };

            foreach (var permission in role.Value)
            {
                Permission permissionEntity = existingPermissions.First(x => x.Code == permission);
                roleEntity.Permissions.Add(permissionEntity);
            }

            rolesEntities.Add(roleEntity);
        }

        await rolesPermissionsRepository.AddRolesWithPermissions(rolesEntities, cancellationToken);
    }

    private async Task SeedAdminAccount()
    {
        var adminExists = await userManager.Users
            .FirstOrDefaultAsync(u => u.Email == _adminOptions.Email);

        if (adminExists is not null)
            return;

        var adminRole = await rolesPermissionsRepository.GetRoleByName(AdminAccount.ADMIN)
                        ?? throw new ApplicationException("Could not find admin role.");

        using var transaction = await unitOfWork.BeginTransaction();

        var fullName = FullName.Create(_adminOptions.UserName, _adminOptions.UserName, _adminOptions.UserName)
            .Value;

        var adminUser = User.CreateAdmin(
            _adminOptions.UserName,
            _adminOptions.Email,
            fullName,
            adminRole);

        if (adminUser.IsFailure)
            throw new ApplicationException(adminUser.Error.Message);

        await userManager.CreateAsync(adminUser.Value, _adminOptions.Password);

        await unitOfWork.SaveChanges();

        transaction.Commit();

        logger.LogInformation("Admin account added to database");
    }
}