using Kyvo.Application.Services.AppService;
using Kyvo.Application.Services.AuditLog;
using Kyvo.Application.Services.IdentityProvider;
using Kyvo.Application.Services.LocalAuthentication;
using Kyvo.Application.Services.Membership;
using Kyvo.Application.Services.Platform;
using Kyvo.Application.Services.Tenant;
using Kyvo.Application.Services.TenantRoles;
using Kyvo.Application.Services.Users;
using Kyvo.Infrastructure.Services.AppService;
using Kyvo.Infrastructure.Services.AuditLog;
using Kyvo.Infrastructure.Services.IdentityProvider;
using Kyvo.Infrastructure.Services.LocalAuthentication;
using Kyvo.Infrastructure.Services.Membership;
using Kyvo.Infrastructure.Services.Platform;
using Kyvo.Infrastructure.Services.Tenant;
using Kyvo.Infrastructure.Services.TenantRoles;
using Kyvo.Infrastructure.Services.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Kyvo.Infrastructure.Extensions;

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
