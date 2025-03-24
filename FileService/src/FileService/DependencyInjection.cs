using System.Reflection;
using Amazon.S3;
using FileService.BackgroundServices;
using FileService.Consumers;
using FileService.Contracts.Options;
using FileService.FilesManagement;
using FileService.VideoProcessing;
using MassTransit;
using MassTransit.Logging;
using MassTransit.Monitoring;
using SachkovTech.Core.Caching;
using SachkovTech.Framework.Authorization;
using SachkovTech.Framework.Endpoints;
using SachkovTech.Framework.Logging;
using SachkovTech.Framework.Observability;
using SachkovTech.Framework.Swagger;

namespace FileService;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddInfrastructure(configuration)
            .AddFramework(configuration);

        services.Configure<VideoProcessOptions>(configuration.GetSection(nameof(VideoProcessOptions)));

        services.AddTransient<VideoProcessor>();
        services.AddTransient<ProcessRunner>();

        return services;
    }

    private static IServiceCollection AddFramework(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplicationLoggingSeq(configuration)
            .AddEndpointsApiExplorer()
            .AddCustomSwagger(configuration)
            .AddSwaggerGen()
            .AddAuthServices(configuration)
            .AddEndpoints(Assembly.GetExecutingAssembly())
            .AddCors()
            .AddObservability(configuration, [InstrumentationOptions.MeterName],
                [DiagnosticHeaders.DefaultListenerName]);

        services.AddHttpContextAccessor()
            .AddScoped<UserScopedData>();

        return services;
    }
}

public static class DependencyInjectionInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMessageBus(configuration)
            .AddMinio(configuration)
            .FileServices(configuration)
            .AddDistributedCache(configuration)
            .AddBackgroundServices();

        return services;
    }

    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<CancelMultipartUploadService>();

        return services;
    }

    private static IServiceCollection FileServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IS3Provider, S3Provider>();

        return services;
    }

    private static IServiceCollection AddMinio(this IServiceCollection services, IConfiguration configuration) =>
        services.AddSingleton<IAmazonS3>(_ =>
        {
            var minioOptions = configuration.GetSection(MinioOptions.MINIO).Get<MinioOptions>()
                               ?? throw new ApplicationException("Missing minio configuration");

            var config = new AmazonS3Config
            {
                ServiceURL = minioOptions.Endpoint, ForcePathStyle = true, UseHttp = true,
            };

            return new AmazonS3Client(minioOptions.AccessKey, minioOptions.SecretKey, config);
        });

    private static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(configure =>
        {
            configure.SetKebabCaseEndpointNameFormatter();

            configure.AddConsumer<LessonVideoUploadedConsumer>(cfg =>
            {
                cfg.UseMessageRetry(r =>
                {
                    r.Ignore<FfmpegProcessingException>();

                    r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
                });
            });

            configure.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(configuration["RabbitMQ:Host"]!), h =>
                {
                    h.Username(configuration["RabbitMQ:UserName"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}