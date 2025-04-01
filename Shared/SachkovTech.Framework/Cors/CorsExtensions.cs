using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SachkovTech.Framework.Cors;

public static class CorsExtensions
{
    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<CorsSettings>(configuration.GetSection(CorsSettings.CORS));
    }

    public static void ConfigureCors(this WebApplication app)
    {
        var corsSettings = app.Services.GetRequiredService<IOptions<CorsSettings>>().Value;

        app.UseCors(config =>
        {
            if (corsSettings.AllowedOrigins.Length > 0)
                config.WithOrigins(corsSettings.AllowedOrigins);

            if (corsSettings.AllowCredentials)
                config.AllowCredentials();

            if (corsSettings.AllowedHeaders.Length > 0 && !corsSettings.AllowedHeaders.Contains("*"))
            {
                config.WithHeaders(corsSettings.AllowedHeaders);
            }
            else
            {
                config.AllowAnyHeader();
            }

            if (corsSettings.AllowedMethods.Length > 0 && !corsSettings.AllowedMethods.Contains("*"))
            {
                config.WithMethods(corsSettings.AllowedMethods);
            }
            else
            {
                config.AllowAnyMethod();
            }
        });
    }
}