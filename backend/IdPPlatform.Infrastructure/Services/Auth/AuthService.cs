using IdPPlatform.Application.Exceptions;
using IdPPlatform.Application.Services.AppService;
using IdPPlatform.Application.Services.Auth;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Application.Services.UserScope;
using IdPPlatform.Domain.Constants;
using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Exceptions;
using IdPPlatform.Domain.Repositories;
using IdPPlatform.Domain.ValueObjects;

namespace IdPPlatform.Infrastructure.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ITenantMembershipRepository _memberships;
    private readonly IAuthSessionRepository _sessions;
    private readonly IApplicationClientRepository _clients;
    private readonly IApplicationRepository _applications;
    private readonly ITenantRepository _tenants;
    private readonly ITenantRoleRepository _roles;
    private readonly IApplicationTenantRepository _applicationTenants;
    private readonly IUserPlatformRoleRepository _userPlatformRoles;
    private readonly IUserScope _userScope;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository users,
        ITenantMembershipRepository memberships,
        IAuthSessionRepository sessions,
        IApplicationClientRepository clients,
        IApplicationRepository applications,
        ITenantRepository tenants,
        ITenantRoleRepository roles,
        IApplicationTenantRepository applicationTenants,
        IUserPlatformRoleRepository userPlatformRoles,
        IUserScope userScope,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _memberships = memberships;
        _sessions = sessions;
        _clients = clients;
        _applications = applications;
        _tenants = tenants;
        _roles = roles;
        _applicationTenants = applicationTenants;
        _userPlatformRoles = userPlatformRoles;
        _userScope = userScope;
        _unitOfWork = unitOfWork;
    }

    public async Task<TenantContextResult> SwitchTenantAsync(
        SwitchTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_userScope.IsAuthenticated || _userScope.UserId == Guid.Empty || !_userScope.SessionId.HasValue)
        {
            throw new UnauthorizedApplicationException(ApplicationErrorMessages.Auth.AuthenticatedSessionRequired);
        }

        var membership = await _memberships.GetByUserIdAndTenantIdWithRolesAsync(
            _userScope.UserId,
            request.TenantId,
            cancellationToken);

        if (membership is null || !membership.IsActive)
        {
            throw new ForbiddenApplicationException(ApplicationErrorMessages.Auth.UserHasNoTenantAccess);
        }

        var session = await _sessions.GetForUpdateAsync(_userScope.SessionId.Value, cancellationToken)
            ?? throw new UnauthorizedApplicationException(ApplicationErrorMessages.Auth.SessionNotFound);

        session.SwitchTenant(membership.TenantId, membership.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _users.GetForUpdateAsync(_userScope.UserId, cancellationToken)
            ?? throw new UnauthorizedApplicationException(ApplicationErrorMessages.Auth.UserNotFound);

        var memberships = await _memberships.ListByUserIdWithTenantAndRolesAsync(user.Id, cancellationToken);
        var platformRoles = await ResolvePlatformRolesAsync(user.Id, cancellationToken);
        return BuildTenantContextResult(user, session, memberships, platformRoles);
    }

    public async Task<TenantContextResult> SubscribeTenantAsync(
        SubscribeTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_userScope.IsAuthenticated || _userScope.UserId == Guid.Empty || !_userScope.SessionId.HasValue)
        {
            throw new UnauthorizedApplicationException(ApplicationErrorMessages.Auth.AuthenticatedSessionRequired);
        }

        var session = await _sessions.GetForUpdateAsync(_userScope.SessionId.Value, cancellationToken)
            ?? throw new UnauthorizedApplicationException(ApplicationErrorMessages.Auth.SessionNotFound);

        if (session.UserId != _userScope.UserId)
        {
            throw new ForbiddenApplicationException(ApplicationErrorMessages.Auth.CannotRevokeAnotherUserSession);
        }

        if (!session.ClientId.HasValue)
        {
            throw new InvalidClientException(ApplicationErrorMessages.Auth.SessionHasNoOAuthClient);
        }

        var client = await _clients.GetByIdAsync(session.ClientId.Value, cancellationToken)
            ?? throw new InvalidClientException(ApplicationErrorMessages.OAuthClient.ClientIdInvalid);

        var application = await _applications.GetByIdAsync(client.ApplicationId, cancellationToken)
            ?? throw new DomainNotFoundException(ApplicationErrorMessages.Application.NotFound);

        var tenantKey = new TenantKey(request.TenantKey);
        if (await _tenants.KeyAlreadyExistsAsync(tenantKey, cancellationToken))
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.Tenant.KeyAlreadyExists);
        }

        var user = await _users.GetForUpdateAsync(_userScope.UserId, cancellationToken)
            ?? throw new DomainNotFoundException(DomainErrorMessages.User.UserNotFound);

        if (!user.IsActive)
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.User.UserInactive);
        }

        var tenant = new Domain.Entities.Tenant(request.TenantName, tenantKey);
        await _tenants.AddAsync(tenant, cancellationToken);

        Domain.Entities.TenantRole? ownerRole = null;
        foreach (var role in TenantRoleDefaults.All)
        {
            var createdRole = new Domain.Entities.TenantRole(
                tenant.Id,
                role.Key,
                role.Name,
                isSystem: true);
            await _roles.AddAsync(createdRole, cancellationToken);

            if (role.Key.Equals(TenantRoleDefaults.Owner, StringComparison.OrdinalIgnoreCase))
            {
                ownerRole = createdRole;
            }
        }

        if (ownerRole is null)
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.TenantRole.AtLeastOneRoleRequired);
        }

        var membership = new TenantMembership(tenant.Id, _userScope.UserId, [ownerRole]);
        await _memberships.AddAsync(membership, cancellationToken);

        var applicationTenant = new ApplicationTenant(
            application.Id,
            tenant.Id,
            request.ExternalCustomerId,
            request.PlanCode);

        if (await _applicationTenants.ExistsAsync(application.Id, tenant.Id, cancellationToken))
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.ApplicationTenant.MappingAlreadyExists);
        }

        await _applicationTenants.AddAsync(applicationTenant, cancellationToken);

        session.SwitchTenant(tenant.Id, membership.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var memberships = await _memberships.ListByUserIdWithTenantAndRolesAsync(user.Id, cancellationToken);
        var platformRoles = await ResolvePlatformRolesAsync(user.Id, cancellationToken);
        return BuildTenantContextResult(user, session, memberships, platformRoles);
    }

    public async Task<IReadOnlyList<AuthSessionDto>> ListActiveSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _sessions.ListActiveByUserIdAsync(userId, cancellationToken);
        return sessions
            .Select(session => new AuthSessionDto
            {
                SessionId = session.Id,
                TenantId = session.TenantId,
                MembershipId = session.MembershipId,
                ClientId = session.ClientId,
                Status = session.Status,
                UserAgent = session.UserAgent,
                IpAddress = session.IpAddress,
                ExpiresAt = session.ExpiresAt,
                LastActivityAt = session.LastActivityAt
            })
            .ToList();
    }

    public async Task RevokeSessionAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessions.GetForUpdateAsync(sessionId, cancellationToken)
            ?? throw new DomainNotFoundException(ApplicationErrorMessages.Auth.SessionNotFound);

        if (session.UserId != userId)
        {
            throw new ForbiddenApplicationException(ApplicationErrorMessages.Auth.CannotRevokeAnotherUserSession);
        }

        session.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<string>> ResolvePlatformRolesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var assignments = await _userPlatformRoles.ListByUserIdAsync(userId, cancellationToken);
        return assignments.Select(x => x.Role.Key).ToList();
    }

    private static TenantContextResult BuildTenantContextResult(
        Domain.Entities.User user,
        AuthSession session,
        IReadOnlyList<TenantMembership> memberships,
        IReadOnlyList<string> platformRoles)
    {
        var membership = memberships.FirstOrDefault(x => x.Id == session.MembershipId);
        return new TenantContextResult
        {
            UserId = user.Id,
            Email = user.Email,
            TenantId = session.TenantId,
            MembershipId = session.MembershipId,
            TenantRoles = membership?.Roles.Select(x => x.Role.Key.Value).ToList() ?? [],
            PlatformRoles = platformRoles,
            Tenants = memberships
                .Select(x => new AuthTenantSummaryDto
                {
                    TenantId = x.TenantId,
                    TenantName = x.Tenant.Name,
                    TenantKey = x.Tenant.Key.Value,
                    Roles = x.Roles.Select(role => role.Role.Key.Value).ToList()
                })
                .ToList()
        };
    }
}
