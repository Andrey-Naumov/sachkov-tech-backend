using System.Reflection;
using AccountService.Api.Providers;
using AccountService.Application;
using AccountService.Infrastructure;
using FluentValidation;
using MassTransit.Logging;
using MassTransit.Monitoring;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Caching;
using SachkovTech.Framework.Authorization;
using SachkovTech.Framework.Logging;
using SachkovTech.Framework.Observability;
using SachkovTech.Framework.Swagger;

namespace AccountService.Api;

public static class DependencyInjection
{
    public static void AddProgramDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        var assemblies = new[]
        {
            typeof(AccountService.Application.DependencyInjection).Assembly,
        };

        services.AddControllers();
        services.AddRouting(options => options.LowercaseUrls = true);

        services
            .AddInfrastructure(configuration)
            .AddApplication(configuration)
            .AddFramework(configuration, assemblies);
    }

    private static IServiceCollection AddFramework(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        services.AddEndpointsApiExplorer()
            .AddApplicationLoggingSeq(configuration)
            .AddValidatorsFromAssemblies(assemblies)
            .AddHandlers(assemblies)
            .AddCustomSwagger(configuration)
            .AddAuthServices(configuration)
            .AddDistributedCache(configuration)
            .AddObservability(configuration, [InstrumentationOptions.MeterName],
                [DiagnosticHeaders.DefaultListenerName]);

        services.AddHttpContextAccessor()
            .AddScoped<UserScopedData>();

        services.AddScoped<HttpContextProvider>();

        return services;
    }
}