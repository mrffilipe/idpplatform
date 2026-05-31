using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kyvo.AspNetCore;

public static class KyvoServiceCollectionExtensions
{
    public static IServiceCollection AddKyvoAuthentication(
        this IServiceCollection services,
        Action<KyvoOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<KyvoOptions>()
            .Configure(configure)
            .PostConfigure(o =>
            {
                o.Authority = o.Authority.TrimEnd('/');
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IKyvoUserContext, KyvoUserContext>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
        services.AddSingleton<IConfigureOptions<AuthorizationOptions>, ConfigureAuthorizationOptions>();

        return services;
    }

    public static IServiceCollection AddKyvoAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = KyvoOptions.SectionName)
    {
        services.AddOptions<KyvoOptions>()
            .Bind(configuration.GetSection(sectionName))
            .PostConfigure(o => o.Authority = o.Authority.TrimEnd('/'));

        services.AddHttpContextAccessor();
        services.AddScoped<IKyvoUserContext, KyvoUserContext>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
        services.AddSingleton<IConfigureOptions<AuthorizationOptions>, ConfigureAuthorizationOptions>();
        return services;
    }

    private sealed class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly KyvoOptions _options;

        public ConfigureJwtBearerOptions(IOptions<KyvoOptions> options) =>
            _options = options.Value;

        public void Configure(string? name, JwtBearerOptions options) => Configure(options);

        public void Configure(JwtBearerOptions options)
        {
            options.Authority = _options.Authority;
            options.Audience = _options.Audience;
            options.RequireHttpsMetadata = false;
            options.BackchannelHttpHandler = DevKyvoCertificateHandler.Create(_options.AllowInvalidCertificate);
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
            options.AddKyvoPolicies();
    }
}
