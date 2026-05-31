using Kyvo.Application.Interfaces;
using Kyvo.Application.Services.Auth;
using Kyvo.Application.Services.Email;
using Kyvo.Application.Services.RefreshTokenHasher;
using Kyvo.Application.Services.Registration;
using Kyvo.Application.Services.TenantResolutionCache;
using Kyvo.Application.Services.TenantRoles;
using Kyvo.Application.Services.UnitOfWork;
using Kyvo.Application.Services.UserScope;
using Kyvo.Infrastructure.Services.Auth;
using Kyvo.Infrastructure.Services.Email;
using Kyvo.Infrastructure.Services.Invite;
using Kyvo.Infrastructure.Services.RefreshTokenHasher;
using Kyvo.Infrastructure.Services.Registration;
using Kyvo.Infrastructure.Services.TenantResolutionCache;
using Kyvo.Infrastructure.Services.TenantRoles;
using Kyvo.Infrastructure.Services.UnitOfWork;
using Kyvo.Infrastructure.Services.UserScope;
using Microsoft.Extensions.DependencyInjection;

namespace Kyvo.Infrastructure.Extensions;

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
        services.AddSingleton<IPasswordPolicy, PasswordPolicy>();
        services.AddScoped<IRegistrationService, RegistrationService>();

        return services;
    }
}
