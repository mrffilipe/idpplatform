using IdPPlatform.Application.Services.Auth;
using IdPPlatform.Application.Services.LocalAuthentication;

namespace IdPPlatform.API.Common;

internal static class LocalLoginResultExtensions
{
    public static ExternalLoginResult ToExternalLoginResult(this LocalLoginResult login) =>
        new()
        {
            UserId = login.UserId,
            Email = login.Email,
            DisplayName = login.DisplayName,
            PlatformRoles = login.PlatformRoles,
            TenantMemberships = login.TenantMemberships
        };
}
