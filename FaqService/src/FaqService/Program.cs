using FaqService;
using SachkovTech.Framework.Endpoints;
using SachkovTech.Framework.Middlewares;
using Serilog;

const string dockerEnvName = "Docker";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services
    .AddProgramDependencies(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionMiddleware();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment(dockerEnvName))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapEndpoints();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseRouting();

app.Run();