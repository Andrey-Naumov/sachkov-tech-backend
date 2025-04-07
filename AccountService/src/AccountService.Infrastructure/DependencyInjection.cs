using AccountService.Application.Database;
using AccountService.Application.Interfaces;
using AccountService.Application.Providers;
using AccountService.Domain.Roles;
using AccountService.Domain.Users;
using AccountService.Infrastructure.Consumers;
using AccountService.Infrastructure.DbContexts;
using AccountService.Infrastructure.Migrator;
using AccountService.Infrastructure.Options;
using AccountService.Infrastructure.Providers;
using AccountService.Infrastructure.Repositories;
using AccountService.Infrastructure.Repository;
using AccountService.Infrastructure.Seeding;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SachkovTech.Core.Database;

namespace AccountService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity()
            .AddRepositories()
            .AddDbContexts(configuration)
            .AddSeeding()
            .ConfigureCustomOptions(configuration)
            .AddMessageBus(configuration)
            .AddProviders()
            .AddMigrators();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    private static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(configure =>
        {
            configure.SetKebabCaseEndpointNameFormatter();

            configure.AddConsumer<AvatarUploadedConsumer>();

            configure.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(configuration["RabbitMQ:Host"]!), h =>
                {
                    h.Username(configuration["RabbitMQ:UserName"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static IServiceCollection AddMigrators(this IServiceCollection services)
    {
        services.AddScoped<IMigrator, AccountsMigrator>();

        return services;
    }

    private static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services
            .AddIdentity<User, Role>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<AccountsDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<RolesRepository>();
        services.AddScoped<IRolesRepository, RolesRepository>();
        services.AddScoped<IRefreshSessionsRepository, RefreshSessionsRepository>();

        return services;
    }

    private static IServiceCollection AddDbContexts(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<AccountsDbContext>(_ =>
            new AccountsDbContext(configuration.GetConnectionString("Database")!));

        services.AddScoped<IAccountsReadDbContext, AccountsDbContext>(_ =>
            new AccountsDbContext(configuration.GetConnectionString("Database")!));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddSeeding(this IServiceCollection services)
    {
        services.AddSingleton<IAutoSeeder, AccountsSeeder>();
        services.AddScoped<AccountsSeederService>();

        return services;
    }

    private static IServiceCollection ConfigureCustomOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AdminOptions>(configuration.GetSection(AdminOptions.ADMIN));

        return services;
    }

    private static IServiceCollection AddProviders(this IServiceCollection services)
    {
        services.AddTransient<ITokenProvider, JwtTokenProvider>();

        return services;
    }
}