using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SachkovTech.Framework.Http;

namespace IssueService.Communication.Lesson;

public static class LessonServiceExtensions
{
    public static IServiceCollection AddLessonHttpCommunication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IssueServiceOptions>(configuration.GetSection(IssueServiceOptions.ISSUE_SERVICE));
        services.AddHttpClient<ILessonService, LessonHttpClient>((sp, config) =>
        {
            var fileOptions = sp.GetRequiredService<IOptions<IssueServiceOptions>>().Value;

            config.BaseAddress = new Uri(fileOptions.Url);
        }).AddHttpMessageHandler<HttpTrackerHandler>();

        return services;
    }
}