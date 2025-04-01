using System.Reflection;
using FileService.Communication;
using FluentValidation;
using MassTransit.Logging;
using MassTransit.Monitoring;
using Microsoft.AspNetCore.Mvc;
using SachkovTech.Core.Abstractions;
using SachkovTech.Core.Caching;
using SachkovTech.Framework.Authorization;
using SachkovTech.Framework.Cors;
using SachkovTech.Framework.Endpoints;
using SachkovTech.Framework.Logging;
using SachkovTech.Framework.Observability;
using SachkovTech.Framework.Swagger;
using SachkovTech.Issues.Application;
using SachkovTech.Issues.Infrastructure;

namespace SachkovTech.Web;

public static class DependencyInjection
{
    public static void AddProgramDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplicationLoggingSeq(configuration);

        var assemblies = new[]
        {
            typeof(SachkovTech.Issues.Application.DependencyInjection).Assembly,
        };

        services.AddControllers();
        services.AddLowerCaseRouting();
        services.AddCors(configuration);

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services
            .AddApplication()
            .AddInfrastructure(configuration)
            .AddFramework(configuration, assemblies);
    }

    private static IServiceCollection AddFramework(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        services.AddEndpointsApiExplorer()
            .AddValidatorsFromAssemblies(assemblies)
            .AddHandlers(assemblies)
            .AddCustomSwagger(configuration)
            .AddAuthServices(configuration)
            .AddDistributedCache(configuration)
            .AddObservability(configuration, [InstrumentationOptions.MeterName],
                [DiagnosticHeaders.DefaultListenerName]);

        services.AddFileHttpCommunication(configuration);

        services.AddHttpContextAccessor()
            .AddScoped<UserScopedData>();

        return services;
    }
}