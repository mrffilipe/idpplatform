using IdPPlatform.Application.Interfaces;
using IdPPlatform.Application.Services.Auth;
using IdPPlatform.Application.Services.Email;
using IdPPlatform.Application.Services.RefreshTokenHasher;
using IdPPlatform.Application.Services.TenantResolutionCache;
using IdPPlatform.Application.Services.TenantRoles;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Application.Services.UserScope;
using IdPPlatform.Infrastructure.Services.Auth;
using IdPPlatform.Infrastructure.Services.Email;
using IdPPlatform.Infrastructure.Services.Invite;
using IdPPlatform.Infrastructure.Services.RefreshTokenHasher;
using IdPPlatform.Infrastructure.Services.TenantResolutionCache;
using IdPPlatform.Infrastructure.Services.TenantRoles;
using IdPPlatform.Infrastructure.Services.UnitOfWork;
using IdPPlatform.Infrastructure.Services.UserScope;
using Microsoft.Extensions.DependencyInjection;

namespace IdPPlatform.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserScope, HttpUserScope>();
        services.AddScoped<IRefreshTokenHasher, RefreshTokenHasher>();
        services.AddScoped<ITenantRoleResolver, TenantRoleResolver>();
        services.AddScoped<ITenantResolutionCache, DistributedTenantResolutionCache>();
        services.AddScoped<IEmailService, AwsSesEmailService>();
        services.AddScoped<IInvitePolicy, InvitePolicy>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
