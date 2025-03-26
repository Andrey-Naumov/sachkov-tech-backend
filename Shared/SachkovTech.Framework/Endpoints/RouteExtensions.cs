using Microsoft.Extensions.DependencyInjection;

namespace SachkovTech.Framework.Endpoints;

public static class RouteExtensions
{
    public static IServiceCollection AddLowerCaseRouting(this IServiceCollection services) =>
        services.AddRouting(options => options.LowercaseUrls = true);
}