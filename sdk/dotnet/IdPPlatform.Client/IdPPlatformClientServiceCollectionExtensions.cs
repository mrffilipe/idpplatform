using IdPPlatform.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdPPlatform.Client;

public static class IdPPlatformClientServiceCollectionExtensions
{
    public static IServiceCollection AddIdPPlatformClient(this IServiceCollection services, Action<IdPPlatformClientOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddHttpClient<IIdPProductClient, IdPProductClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<IdPPlatformClientOptions>>().Value;
                client.BaseAddress = new Uri(options.Authority.TrimEnd('/') + "/");
            })
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<IdPPlatformClientOptions>>().Value;
                return DevIdpCertificateHandler.Create(options.AllowInvalidCertificate);
            });

        return services;
    }

    public static IServiceCollection AddIdPPlatformClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = IdPPlatformClientOptions.SectionName)
    {
        services.Configure<IdPPlatformClientOptions>(configuration.GetSection(sectionName));
        return services.AddIdPPlatformClient();
    }

    /// <summary>
    /// Forwards the Bearer token from the current HTTP request to IdP API calls.
    /// </summary>
    public static string? GetUserAccessToken(this IHttpContextAccessor accessor)
    {
        var header = accessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(header)
            || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return header["Bearer ".Length..].Trim();
    }
}
