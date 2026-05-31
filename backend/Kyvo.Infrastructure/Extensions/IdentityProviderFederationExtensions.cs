using Kyvo.Application.Services.Auth;
using Kyvo.Application.Services.IdentityProvider;
using Kyvo.Infrastructure.Services.Auth;
using Kyvo.Infrastructure.Services.ExternalIdentityProvider;
using Microsoft.Extensions.DependencyInjection;

namespace Kyvo.Infrastructure.Extensions;

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
