using Kyvo.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kyvo.Client;

public static class KyvoClientServiceCollectionExtensions
{
    public static IServiceCollection AddKyvoClient(this IServiceCollection services, Action<KyvoClientOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddHttpClient<IKyvoProductClient, KyvoProductClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<KyvoClientOptions>>().Value;
                client.BaseAddress = new Uri(options.Authority.TrimEnd('/') + "/");
            })
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<KyvoClientOptions>>().Value;
                return DevKyvoCertificateHandler.Create(options.AllowInvalidCertificate);
            });

        return services;
    }

    public static IServiceCollection AddKyvoClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = KyvoClientOptions.SectionName)
    {
        services.Configure<KyvoClientOptions>(configuration.GetSection(sectionName));
        return services.AddKyvoClient();
    }

    /// <summary>
    /// Forwards the Bearer token from the current HTTP request to Kyvo API calls.
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
