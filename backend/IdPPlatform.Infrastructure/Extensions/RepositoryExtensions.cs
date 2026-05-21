using IdPPlatform.Domain.Repositories;
using IdPPlatform.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace IdPPlatform.Infrastructure.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserCredentialRepository, UserCredentialRepository>();
        services.AddScoped<IUserPlatformRoleRepository, UserPlatformRoleRepository>();
        services.AddScoped<IPlatformRoleRepository, PlatformRoleRepository>();
        services.AddScoped<IIdentityProviderRepository, IdentityProviderRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenantRoleRepository, TenantRoleRepository>();
        services.AddScoped<ITenantMembershipRepository, TenantMembershipRepository>();
        services.AddScoped<IExternalIdentityRepository, ExternalIdentityRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IApplicationClientRepository, ApplicationClientRepository>();
        services.AddScoped<IApplicationTenantRepository, ApplicationTenantRepository>();
        services.AddScoped<IAuthSessionRepository, AuthSessionRepository>();
        services.AddScoped<ITenantInviteRepository, TenantInviteRepository>();
        services.AddScoped<IPlatformConfigurationRepository, PlatformConfigurationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IOidcAuthorizationCodeRepository, OidcAuthorizationCodeRepository>();
        services.AddScoped<IOidcRefreshTokenRepository, OidcRefreshTokenRepository>();

        return services;
    }
}
