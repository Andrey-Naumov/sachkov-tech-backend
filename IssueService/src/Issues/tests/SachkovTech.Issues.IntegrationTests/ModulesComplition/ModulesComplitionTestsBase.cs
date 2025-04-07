using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Infrastructure.DbContexts;

namespace SachkovTech.Issues.IntegrationTests.ModulesComplition;

public class ModulesComplitionTestsBase : IClassFixture<ModulesComplitionTestWebFactory>, IAsyncLifetime
{
    protected readonly ModulesComplitionTestWebFactory Factory;
    protected readonly IssuesDbContext DbContext;
    protected readonly IIssuesReadDbContext ReadDbContext;
    protected readonly IServiceScope Scope;
    protected readonly Fixture Fixture;

    private readonly Func<Task> _resetDatabase;

    protected ModulesComplitionTestsBase(ModulesComplitionTestWebFactory factory)
    {
        _resetDatabase = factory.ResetDatabaseAsync;
        Scope = factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<IssuesDbContext>();
        ReadDbContext = Scope.ServiceProvider.GetRequiredService<IIssuesReadDbContext>();
        Fixture = new Fixture();
        Factory = factory;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _resetDatabase();
        Scope.Dispose();
    }

    protected async Task SeedUserModule(Guid moduleId, Guid userId)
    {
        var userModule = Fixture.CreateUserModule(moduleId, userId);

        await DbContext.UserModules.AddAsync(userModule);

        await DbContext.SaveChangesAsync();
    }
    
    protected async Task<Guid> SeedModule()
    {
        var module = Fixture.CreateModule();

        await DbContext.Modules.AddAsync(module);

        await DbContext.SaveChangesAsync();

        return module.Id;
    }
}