using System.Security.Claims;
using Kyvo.Application.Services.Auth;
using Kyvo.Application.Services.Oidc;
using Kyvo.Domain.Enums;
using Kyvo.Domain.Repositories;

namespace Kyvo.Infrastructure.Services.Oidc;

public sealed class OidcClaimsService : IOidcClaimsService
{
    private readonly IAuthSessionRepository _sessions;
    private readonly IUserRepository _users;
    private readonly ITenantMembershipRepository _memberships;
    private readonly IUserPlatformRoleRepository _userPlatformRoles;

    public OidcClaimsService(
        IAuthSessionRepository sessions,
        IUserRepository users,
        ITenantMembershipRepository memberships,
        IUserPlatformRoleRepository userPlatformRoles)
    {
        _sessions = sessions;
        _users = users;
        _memberships = memberships;
        _userPlatformRoles = userPlatformRoles;
    }

    public async Task<IReadOnlyList<Claim>?> TryBuildClaimsAsync(
        Guid sessionId,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetForUpdateAsync(sessionId, cancellationToken);
        if (session is null || session.Status != SessionStatus.Active)
        {
            return null;
        }

        var user = await _users.GetForUpdateAsync(session.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var memberships = await _memberships.ListByUserIdWithTenantAndRolesAsync(user.Id, cancellationToken);
        var tenantRoles = memberships
            .FirstOrDefault(m => m.Id == session.MembershipId)
            ?.Roles.Select(r => r.Role.Key.Value)
            .ToList() ?? [];

        var platformRoleAssignments = await _userPlatformRoles.ListByUserIdAsync(user.Id, cancellationToken);
        var platformRoles = platformRoleAssignments.Select(x => x.Role.Key).ToList();

        var login = new ExternalLoginResult
        {
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PlatformRoles = platformRoles,
            TenantMemberships = memberships
                .Select(m => new ExternalLoginTenantMembership
                {
                    TenantId = m.TenantId,
                    MembershipId = m.Id,
                    Roles = m.Roles.Select(r => r.Role.Key.Value).ToList()
                })
                .ToList()
        };

        return OidcClaimsBuilder.Build(
            login,
            session.Id,
            session.TenantId,
            session.MembershipId,
            tenantRoles);
    }
}
