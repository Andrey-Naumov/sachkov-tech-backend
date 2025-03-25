using SachkovTech.Framework.Endpoints;
using SachkovTech.Framework.Middlewares;
using Serilog;
using TagService;
using TagService.Infrastructure;

const string dockerEnv = "Docker";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services.AddProgramDependencies(builder.Configuration);
builder.Services.AddScoped<ApplicationDbContext>();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionMiddleware();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment(dockerEnv))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(config =>
{
    config.WithOrigins("http://localhost:5144")
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod();
});

app.UseAuthentication();
app.UseScopeDataMiddleware();
app.UseAuthorization();
app.MapEndpoints();

app.Run();