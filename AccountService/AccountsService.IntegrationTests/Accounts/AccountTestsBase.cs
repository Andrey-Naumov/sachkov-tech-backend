using System.Text.Json;
using AccountService.Application.Database;
using AccountService.Application.Interfaces;
using AccountService.Domain.Roles;
using AccountService.Domain.Users;
using AccountService.Infrastructure.DbContexts;
using AccountService.Infrastructure.Options;
using AutoFixture;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AccountsService.IntegrationTests.Accounts;

public abstract class AccountTestsBase : IClassFixture<IntegrationTestsWebFactory>, IAsyncLifetime
{
    protected readonly IntegrationTestsWebFactory Factory;
    protected readonly IServiceScope Scope;
    protected readonly AccountsDbContext DbContext;
    protected readonly IAccountsReadDbContext ReadDbContext;
    protected readonly UserManager<User> UserManager;
    protected readonly RoleManager<Role> RoleManager;
    protected readonly IRefreshSessionsRepository RefreshSessionManager;
    protected readonly Fixture Fixture;

    private readonly Func<Task> _resetDatabase;

    protected AccountTestsBase(IntegrationTestsWebFactory factory)
    {
        _resetDatabase = factory.ResetDatabaseAsync;
        Scope = factory.Services.CreateScope();

        UserManager = Scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        RoleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        RefreshSessionManager = Scope.ServiceProvider.GetRequiredService<IRefreshSessionsRepository>();

        DbContext = Scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
        ReadDbContext = Scope.ServiceProvider.GetRequiredService<IAccountsReadDbContext>();

        Fixture = new Fixture();
        Factory = factory;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDatabase();
        Scope.Dispose();
    }

    protected async Task<Guid> SeedUser()
    {
        var role = RoleManager.Roles
            .FirstOrDefault(r => r.Name == ParticipantAccount.PARTICIPANT);

        if (role == null)
        {
            throw new InvalidOperationException("Role not found: Participant");
        }

        var user = User.CreateParticipant("UserName", "Email@email.com", role).Value;

        await UserManager.CreateAsync(user, "Password1!");

        await DbContext.SaveChangesAsync();

        return user.Id;
    }

    protected async Task SeedRoles()
    {
        var json = await File.ReadAllTextAsync("etc/accounts.json");

        var seedData = JsonSerializer.Deserialize<RolesPermissionsToSeed>(json)
                       ?? throw new ApplicationException("Could not deserialize role permission config.");

        foreach (var roleName in seedData.Roles.Keys)
        {
            var role = await RoleManager.FindByNameAsync(roleName);

            if (role is null)
            {
                await RoleManager.CreateAsync(new Role { Name = roleName });
            }
        }

        await DbContext.SaveChangesAsync();
    }
}