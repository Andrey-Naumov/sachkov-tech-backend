using FileService;
using FileService.Hubs;
using SachkovTech.Framework.Cors;
using SachkovTech.Framework.Endpoints;
using SachkovTech.Framework.Middlewares;
using Serilog;

const string dockerEnv = "Docker";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddProgramDependencies(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionMiddleware();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment(dockerEnv))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.ConfigureCors();

app.UseAuthentication();
app.UseScopeDataMiddleware();
app.UseAuthorization();
app.MapEndpoints();

app.MapHub<VideoProcessingHub>("/video-processing");

app.Run();

namespace FileService
{
    public partial class Program;
}