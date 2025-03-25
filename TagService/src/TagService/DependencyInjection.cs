using System.Reflection;
using MassTransit.Logging;
using MassTransit.Monitoring;
using SachkovTech.Framework.Authorization;
using SachkovTech.Framework.Endpoints;
using SachkovTech.Framework.Logging;
using SachkovTech.Framework.Observability;
using SachkovTech.Framework.Swagger;

namespace TagService;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddFramework(configuration);

        return services;
    }
    
    private static IServiceCollection AddFramework(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplicationLoggingSeq(configuration)
            .AddEndpointsApiExplorer()
            .AddCustomSwagger(configuration)
            .AddSwaggerGen()
            .AddAuthServices(configuration)
            .AddEndpoints(Assembly.GetExecutingAssembly())
            .AddCors()
            .AddObservability(configuration, [InstrumentationOptions.MeterName],
                [DiagnosticHeaders.DefaultListenerName]);;

        services.AddHttpContextAccessor()
            .AddScoped<UserScopedData>();

        return services;
    }
}