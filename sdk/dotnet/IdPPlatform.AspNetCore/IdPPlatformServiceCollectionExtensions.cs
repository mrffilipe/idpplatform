using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdPPlatform.AspNetCore;

public static class IdPPlatformServiceCollectionExtensions
{
    public static IServiceCollection AddIdPPlatformAuthentication(
        this IServiceCollection services,
        Action<IdPPlatformOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<IdPPlatformOptions>()
            .Configure(configure)
            .PostConfigure(o =>
            {
                o.Authority = o.Authority.TrimEnd('/');
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IIdPUserContext, IdPUserContext>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
        services.AddSingleton<IConfigureOptions<AuthorizationOptions>, ConfigureAuthorizationOptions>();

        return services;
    }

    public static IServiceCollection AddIdPPlatformAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = IdPPlatformOptions.SectionName)
    {
        services.AddOptions<IdPPlatformOptions>()
            .Bind(configuration.GetSection(sectionName))
            .PostConfigure(o => o.Authority = o.Authority.TrimEnd('/'));

        services.AddHttpContextAccessor();
        services.AddScoped<IIdPUserContext, IdPUserContext>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
        services.AddSingleton<IConfigureOptions<AuthorizationOptions>, ConfigureAuthorizationOptions>();
        return services;
    }

    private sealed class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly IdPPlatformOptions _options;

        public ConfigureJwtBearerOptions(IOptions<IdPPlatformOptions> options) =>
            _options = options.Value;

        public void Configure(string? name, JwtBearerOptions options) => Configure(options);

        public void Configure(JwtBearerOptions options)
        {
            options.Authority = _options.Authority;
            options.Audience = _options.Audience;
            options.RequireHttpsMetadata = false;
            options.BackchannelHttpHandler = DevIdpCertificateHandler.Create(_options.AllowInvalidCertificate);
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _options.Authority,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                NameClaimType = "sub",
                RoleClaimType = "trole"
            };
        }
    }

    private sealed class ConfigureAuthorizationOptions : IConfigureOptions<AuthorizationOptions>
    {
        public void Configure(AuthorizationOptions options) =>
            options.AddIdPPlatformPolicies();
    }
}
