using AutoFixture;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Domain.Module;
using SachkovTech.Issues.Domain.ValueObjects;
using SachkovTech.Issues.Domain.ValueObjects.Ids;
using SachkovTech.Issues.Infrastructure.DbContexts;

namespace SachkovTech.Issues.IntegrationTests.Lessons;

public class LessonsTestsBase : IClassFixture<LessonTestWebFactory>, IAsyncLifetime
{
    protected readonly LessonTestWebFactory Factory;
    protected readonly IssuesDbContext DbContext;
    protected readonly IIssuesReadDbContext ReadDbContext;
    protected readonly IServiceScope Scope;
    protected readonly Fixture Fixture;
    protected ITestHarness MasstransitHarness;

    private readonly Func<Task> _resetDatabase;

    protected LessonsTestsBase(LessonTestWebFactory factory)
    {
        _resetDatabase = factory.ResetDatabaseAsync;

        MasstransitHarness = factory.Services.GetRequiredService<ITestHarness>();
        Scope = factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<IssuesDbContext>();
        ReadDbContext = Scope.ServiceProvider.GetRequiredService<IIssuesReadDbContext>();
        Fixture = new Fixture();
        Factory = factory;
    }

    public async Task InitializeAsync()
    {
        await MasstransitHarness.Start();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _resetDatabase();
        await MasstransitHarness.Stop();
        Scope.Dispose();
    }

    protected async Task<Guid> SeedModule()
    {
        var module = new Module(
            ModuleId.NewModuleId(),
            Title.Create("title").Value,
            Description.Create("description").Value);

        await DbContext.Modules.AddAsync(module);

        await DbContext.SaveChangesAsync();

        return module.Id;
    }

    protected async Task SeedUserModule(Guid moduleId, Guid userId)
    {
        var userModule = Fixture.CreateUserModule(moduleId, userId);

        await DbContext.UserModules.AddAsync(userModule);

        await DbContext.SaveChangesAsync();
    }
}