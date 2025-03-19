using System.Reflection;
using FaqService;
using SachkovTech.Framework.Endpoints;
using SachkovTech.Framework.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddLogging(builder.Configuration)
    .AddEndpoints(Assembly.GetExecutingAssembly())
    .AddDbContext()
    .AddRepositories()
    .AddElasticSearch(builder.Configuration);

var app = builder.Build();

app.UseExceptionMiddleware();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapEndpoints();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRouting();

app.MapControllers();

app.Run();