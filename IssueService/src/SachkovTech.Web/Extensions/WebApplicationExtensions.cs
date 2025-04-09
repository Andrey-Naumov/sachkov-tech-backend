using SachkovTech.Core.Database;
using SachkovTech.Framework.Cors;
using SachkovTech.Framework.Middlewares;
using SachkovTech.Issues.Presentation.Hubs;
using Serilog;

namespace SachkovTech.Web.Extensions;

public static class WebApplicationExtensions
{
    public static async Task Configure(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
        {
            await app.Services.RunMigrations();
            await app.Services.RunAutoSeeding();

            app.UseOpenTelemetryPrometheusScrapingEndpoint();
        }

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseExceptionMiddleware();
        app.UseSerilogRequestLogging();
        app.ConfigureCors();
        app.UseAuthentication();
        app.UseScopeDataMiddleware();
        app.UseAuthorization();
        app.MapControllers();

        if (app.Environment.IsEnvironment("Docker"))
        {
            app.MapGet("/", () => "Hello World!");
        }

        app.MapHub<LessonVideoProcessingHub>("/api/hubs/lessons/video-processing");
    }
}