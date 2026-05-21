using IdPPlatform.Application.Services.Auth;
using IdPPlatform.Application.Services.IdentityProvider;
using IdPPlatform.Infrastructure.Services.Auth;
using IdPPlatform.Infrastructure.Services.ExternalIdentityProvider;
using Microsoft.Extensions.DependencyInjection;

namespace IdPPlatform.Infrastructure.Extensions;

public static class IdentityProviderFederationExtensions
{
    public static IServiceCollection AddIdentityProviderFederation(this IServiceCollection services)
    {
        services.AddScoped<IExternalLoginService, ExternalLoginService>();
        services.AddScoped<IIdentityProviderTokenValidatorFactory, IdentityProviderTokenValidatorFactory>();
        services.AddScoped<IIdentityProviderConfigValidator, IdentityProviderConfigValidator>();
        services.AddScoped<FirebaseTokenValidator>();
        services.AddScoped<CognitoTokenValidator>();
        services.AddScoped<GenericTokenValidator>();

        return services;
    }
}
