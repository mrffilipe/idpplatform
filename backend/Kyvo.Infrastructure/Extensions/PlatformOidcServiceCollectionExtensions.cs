using System.Text.Json;
using Kyvo.Application.Services.Oidc;
using Kyvo.Infrastructure.Configurations;
using Kyvo.Infrastructure.Services.Oidc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kyvo.Infrastructure.Extensions;

public static class PlatformOidcServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformOidc(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IJwtSigningService, JwtSigningService>();
        services.AddScoped<IOidcClientValidator, OidcClientValidator>();
        services.AddScoped<IOidcClaimsService, OidcClaimsService>();
        services.AddScoped<IOidcAuthorizationService, OidcAuthorizationService>();
        services.AddScoped<IOidcTokenService, OidcTokenService>();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logout";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
            })
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IJwtSigningService>((options, signing) =>
            {
                options.Authority = signing.Issuer;
                options.Audience = signing.Audience;
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = signing.Issuer,
                    ValidAudience = signing.Audience,
                    IssuerSigningKey = signing.SigningKey,
                    NameClaimType = "sub",
                    RoleClaimType = "trole",
                    ValidateAudience = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";

                        var problem = new
                        {
                            type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                            title = "Unauthorized",
                            status = StatusCodes.Status401Unauthorized,
                            detail = string.IsNullOrWhiteSpace(context.ErrorDescription)
                                ? "Bearer token is missing or invalid."
                                : context.ErrorDescription
                        };

                        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                    }
                };
            });

        return services;
    }
}
