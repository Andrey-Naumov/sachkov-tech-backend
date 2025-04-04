using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using SachkovTech.Framework.Authorization;
using Volo.Abp.Authorization;

namespace SachkovTech.Framework.Endpoints;

public static class PolicyExtension
{
    public static IEndpointConventionBuilder RequirePermissions<TBuilder>(
        this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(policy
            => policy
                .AddAuthenticationSchemes(SecretKeyDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme)
                .AddRequirements(new PermissionAttribute(permissions.ToString())));
    }
}