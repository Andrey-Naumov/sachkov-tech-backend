using SachkovTech.Core.Database;
using SachkovTech.Framework.Cors;
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
}