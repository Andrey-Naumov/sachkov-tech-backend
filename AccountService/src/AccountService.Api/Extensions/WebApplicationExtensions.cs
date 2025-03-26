using AccountService.Api.Cors;
using Microsoft.Extensions.Options;
using SachkovTech.Core.Database;
using SachkovTech.Framework.Middlewares;
using Serilog;

namespace AccountService.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task Configure(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            await app.Services.RunMigrations();
            await app.Services.RunAutoSeeding();

            app.UseOpenTelemetryPrometheusScrapingEndpoint();
        }

        app.UseExceptionMiddleware();
        app.UseSerilogRequestLogging();
        app.ConfigureCors();
        app.UseAuthentication();
        app.UseScopeDataMiddleware();
        app.UseAuthorization();
        app.MapControllers();
    }

    private static void ConfigureCors(this WebApplication app)
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