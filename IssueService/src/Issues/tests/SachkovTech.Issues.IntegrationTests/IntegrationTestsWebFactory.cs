using System.Data.Common;
using DotNet.Testcontainers.Builders;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using SachkovTech.Core.Database;
using SachkovTech.Issues.Application.Interfaces;
using SachkovTech.Issues.Infrastructure.Consumers;
using SachkovTech.Issues.Infrastructure.DbContexts;
using SachkovTech.Web;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace SachkovTech.Issues.IntegrationTests;

public class IntegrationTestsWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("sachkov_tech")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder()
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
        .Build();

    private Respawner _respawner = default!;
    private DbConnection _dbConnection = default!;

    public async Task InitializeAsync()
    {
        await _rabbitMqContainer.StartAsync();
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var issuesDbContext = scope.ServiceProvider.GetRequiredService<IssuesDbContext>();

        await issuesDbContext.Database.EnsureDeletedAsync();
        await issuesDbContext.Database.EnsureCreatedAsync();

        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await InitializeRespawner();
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public new async Task DisposeAsync()
    {
        await _rabbitMqContainer.StopAsync();
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(ConfigureDefaultServices);
    }

    protected virtual void ConfigureDefaultServices(IServiceCollection services)
    {
        services.RemoveAll(typeof(IssuesDbContext));
        services.RemoveAll(typeof(IIssuesReadDbContext));
        services.RemoveAll(typeof(ISqlConnectionFactory));

        services.AddScoped<IssuesDbContext>(_ =>
            new IssuesDbContext(_dbContainer.GetConnectionString()));

        services.AddScoped<IIssuesReadDbContext, IssuesDbContext>(_ =>
            new IssuesDbContext(_dbContainer.GetConnectionString()));

        services.AddScoped<ISqlConnectionFactory>(_ =>
            new SqlConnectionFactory(_dbContainer.GetConnectionString()));

        services.AddMassTransitTestHarness(configure =>
        {
            configure.AddConsumer<LessonVideoProcessedConsumer>();

            configure.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(_rabbitMqContainer.GetConnectionString());

                cfg.ConfigureEndpoints(context);
            });
        });
    }

    private async Task InitializeRespawner()
    {
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres, SchemasToInclude = ["issues"]
            });
    }
}