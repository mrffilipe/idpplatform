using Kyvo.Client.Models;

namespace Kyvo.Client;

public interface IKyvoProductClient
{
    IKyvoAuthApi Auth { get; }

    IKyvoUsersApi Users { get; }

    IKyvoTenantsApi Tenants { get; }

    IKyvoMembershipsApi Memberships { get; }

    IKyvoTenantRolesApi TenantRoles { get; }

    IKyvoAuditLogsApi AuditLogs { get; }
}

public interface IKyvoAuthApi
{
    Task<SubscribeTenantResult> SubscribeAsync(
        string userAccessToken,
        SubscribeTenantRequest request,
        CancellationToken cancellationToken = default);

    Task<TenantContextResult> SwitchTenantAsync(
        string userAccessToken,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuthSessionDto>> ListSessionsAsync(
        string userAccessToken,
        CancellationToken cancellationToken = default);

    Task RevokeSessionAsync(
        string userAccessToken,
        Guid sessionId,
        CancellationToken cancellationToken = default);
}

public interface IKyvoUsersApi
{
    Task<UserDto> GetMeAsync(string userAccessToken, CancellationToken cancellationToken = default);

    Task UpdateMeAsync(
        string userAccessToken,
        UpdateUserProfileBody body,
        CancellationToken cancellationToken = default);

    Task<PagedResult<UserMembershipDto>> ListMyMembershipsAsync(
        string userAccessToken,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}

public interface IKyvoTenantsApi
{
    Task<PagedResult<TenantDto>> ListAsync(
        string userAccessToken,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<TenantDto> GetByIdAsync(
        string userAccessToken,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        string userAccessToken,
        Guid tenantId,
        UpdateTenantBody body,
        CancellationToken cancellationToken = default);

    Task<CreatedIdResponse> InviteMemberAsync(
        string userAccessToken,
        Guid tenantId,
        InviteMemberBody body,
        CancellationToken cancellationToken = default);

    Task<CreatedMembershipIdResponse> AcceptInviteAsync(
        string userAccessToken,
        AcceptInviteBody body,
        CancellationToken cancellationToken = default);
}

public interface IKyvoMembershipsApi
{
    Task<CreatedIdResponse> CreateAsync(
        string userAccessToken,
        Guid tenantId,
        CreateMembershipBody body,
        CancellationToken cancellationToken = default);

    Task<PagedResult<MembershipDto>> ListByTenantAsync(
        string userAccessToken,
        Guid tenantId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task UpdateRolesAsync(
        string userAccessToken,
        Guid membershipId,
        UpdateMembershipRolesBody body,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        string userAccessToken,
        Guid membershipId,
        CancellationToken cancellationToken = default);
}

public interface IKyvoTenantRolesApi
{
    Task<PagedResult<TenantRoleDto>> ListAsync(
        string userAccessToken,
        Guid tenantId,
        bool includeInactive = false,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<CreatedIdResponse> CreateAsync(
        string userAccessToken,
        Guid tenantId,
        CreateTenantRoleBody body,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        string userAccessToken,
        Guid roleId,
        UpdateTenantRoleBody body,
        CancellationToken cancellationToken = default);
}

public interface IKyvoAuditLogsApi
{
    Task<PagedResult<AuditLogItemDto>> ListAsync(
        string userAccessToken,
        ListAuditLogsQuery? query = null,
        CancellationToken cancellationToken = default);
}
