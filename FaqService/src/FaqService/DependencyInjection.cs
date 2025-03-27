using System.Reflection;
using FaqService.Infrastructure;
using FaqService.Infrastructure.Repositories;
using MassTransit;
using Nest;
using SachkovTech.Framework.Authorization;
using SachkovTech.Framework.Endpoints;
using SachkovTech.Framework.Logging;

namespace FaqService;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(
        this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddApplicationLoggingSeq(configuration)
            .AddEndpoints(Assembly.GetExecutingAssembly())
            .AddDbContext()
            .AddAuthServices(configuration)
            .AddRepositories()
            .AddElasticSearch(configuration)
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddMessageBus(configuration);
    }

    public static IServiceCollection AddDbContext(this IServiceCollection services)
    {
        return services.AddScoped<ApplicationDbContext>();
    }

    public static IServiceCollection AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
    {
        var elasticSearchSettings = configuration.GetConnectionString("ElasticSearch");
        var settings = new ConnectionSettings(new Uri(elasticSearchSettings!))
            .DefaultIndex("questions");

        var client = new ElasticClient(settings);

        return services.AddSingleton<IElasticClient>(client);
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<SearchRepository>()
            .AddScoped<UnitOfWork>();
    }

    private static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(configure =>
        {
            configure.SetKebabCaseEndpointNameFormatter();

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
}