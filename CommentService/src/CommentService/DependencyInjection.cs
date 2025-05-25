using CommentService.Infrastructure;
using SachkovTech.Core.Database;
using SachkovTech.Framework.Authorization;
using SachkovTech.Framework.Endpoints;
using SachkovTech.Framework.Logging;
using SachkovTech.Framework.Swagger;
using System.Reflection;

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
        services
            .AddApplicationLoggingSeq(configuration)
            .AddEndpointsApiExplorer()
            .AddCustomSwagger(configuration)
            .AddSwaggerGen()
            .AddAuthServices(configuration)
            .AddEndpoints(Assembly.GetExecutingAssembly())
            .AddCors()
            .AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

        services.AddHttpContextAccessor()
            .AddScoped<UserScopedData>();

        return services;
    }
}