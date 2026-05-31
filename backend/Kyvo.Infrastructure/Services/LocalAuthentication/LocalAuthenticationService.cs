using Kyvo.Application.Services.Auth;
using Kyvo.Application.Services.LocalAuthentication;
using Kyvo.Domain.Repositories;

namespace Kyvo.Infrastructure.Services.LocalAuthentication;

public sealed class LocalAuthenticationService : ILocalAuthenticationService
{
    private readonly IUserRepository _users;
    private readonly IUserCredentialRepository _userCredentials;
    private readonly IUserPlatformRoleRepository _userPlatformRoles;
    private readonly ITenantMembershipRepository _memberships;

    public LocalAuthenticationService(
        IUserRepository users,
        IUserCredentialRepository userCredentials,
        IUserPlatformRoleRepository userPlatformRoles,
        ITenantMembershipRepository memberships)
    {
        _users = users;
        _userCredentials = userCredentials;
        _userPlatformRoles = userPlatformRoles;
        _memberships = memberships;
    }

    public async Task<LocalLoginResult?> LoginAsync(
        LocalLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var credential = await _userCredentials.GetByUserIdAsync(user.Id, cancellationToken);
        if (credential is null)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, credential.PasswordHash))
        {
            return null;
        }

        var platformRoleAssignments = await _userPlatformRoles.ListByUserIdAsync(user.Id, cancellationToken);
        var platformRoles = platformRoleAssignments.Select(x => x.Role.Key).ToList();

        var membershipList = await _memberships.ListByUserIdWithTenantAndRolesAsync(user.Id, cancellationToken);

        return new LocalLoginResult
        {
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PlatformRoles = platformRoles,
            TenantMemberships = membershipList
                .Select(m => new ExternalLoginTenantMembership
                {
                    TenantId = m.TenantId,
                    MembershipId = m.Id,
                    Roles = m.Roles.Select(r => r.Role.Key.Value).ToList()
                })
                .ToList()
        };
    }
}
