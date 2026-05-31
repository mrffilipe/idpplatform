using Kyvo.Application.Services.Auth;
using Kyvo.Application.Services.LocalAuthentication;

namespace Kyvo.API.Common;

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
