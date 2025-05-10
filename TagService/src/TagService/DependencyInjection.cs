using System.Reflection;
using IssueService.Communication.Lesson;
using MassTransit;
using MassTransit.Logging;
using MassTransit.Monitoring;
using SachkovTech.Framework.Authorization;
using SachkovTech.Framework.Cors;
using SachkovTech.Framework.Endpoints;
using SachkovTech.Framework.Logging;
using SachkovTech.Framework.Observability;
using SachkovTech.Framework.Swagger;
using SharedKernel.Exeptions;
using TagService.Consumers;

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
            .AddMessageBus(configuration)
            .AddEndpointsApiExplorer()
            .AddCustomSwagger(configuration)
            .AddSwaggerGen()
            .AddAuthServices(configuration)
            .AddEndpoints(Assembly.GetExecutingAssembly())
            .AddCors()
            .AddObservability(configuration, [InstrumentationOptions.MeterName],
                [DiagnosticHeaders.DefaultListenerName]);

        services.AddLessonHttpCommunication(configuration);

        services.AddHttpContextAccessor()
            .AddScoped<UserScopedData>();

        return services;
    }

    private static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(configure =>
        {
            configure.SetKebabCaseEndpointNameFormatter();

            configure.AddConsumer<LessonCreatedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r =>
                {
                    r.Ignore<NotFoundException>();

                    r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
                });
            });
            
            configure.AddConsumer<TagsAssignedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r =>
                {
                    r.Ignore<NotFoundException>();

                    r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
                });
            });
            
            configure.AddConsumer<TagsUnAssignedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r =>
                {
                    r.Ignore<NotFoundException>();

                    r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
                });
            });

            configure.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(configuration["RabbitMQ:Host"]!), h =>
                {
                    h.Username(configuration["RabbitMQ:UserName"]!);
                    h.Password(configuration["RabbitMQ:PasswoЁrd"]!);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}