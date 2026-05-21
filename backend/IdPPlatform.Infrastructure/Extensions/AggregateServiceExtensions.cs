using IdPPlatform.Application.Services.AppService;
using IdPPlatform.Application.Services.AuditLog;
using IdPPlatform.Application.Services.IdentityProvider;
using IdPPlatform.Application.Services.LocalAuthentication;
using IdPPlatform.Application.Services.Membership;
using IdPPlatform.Application.Services.Platform;
using IdPPlatform.Application.Services.Tenant;
using IdPPlatform.Application.Services.TenantRoles;
using IdPPlatform.Application.Services.Users;
using IdPPlatform.Infrastructure.Services.AppService;
using IdPPlatform.Infrastructure.Services.AuditLog;
using IdPPlatform.Infrastructure.Services.IdentityProvider;
using IdPPlatform.Infrastructure.Services.LocalAuthentication;
using IdPPlatform.Infrastructure.Services.Membership;
using IdPPlatform.Infrastructure.Services.Platform;
using IdPPlatform.Infrastructure.Services.Tenant;
using IdPPlatform.Infrastructure.Services.TenantRoles;
using IdPPlatform.Infrastructure.Services.Users;
using Microsoft.Extensions.DependencyInjection;

namespace IdPPlatform.Infrastructure.Extensions;

public static class AggregateServiceExtensions
{
    public static IServiceCollection AddAggregateServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITenantRoleService, TenantRoleService>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IPlatformService, PlatformService>();
        services.AddScoped<IIdentityProviderService, IdentityProviderService>();
        services.AddScoped<ILocalAuthenticationService, LocalAuthenticationService>();

        return services;
    }
}
