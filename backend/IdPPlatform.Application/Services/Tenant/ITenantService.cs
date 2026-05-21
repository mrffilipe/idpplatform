using IdPPlatform.Application.Common;

namespace IdPPlatform.Application.Services.Tenant;

public interface ITenantService
{
    Task<Guid> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateTenantRequest request, CancellationToken cancellationToken = default);

    Task<TenantDto?> GetByIdAsync(GetTenantByIdRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<TenantDto>> ListByUserAsync(
        ListTenantsByUserRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> InviteMemberAsync(InviteMemberRequest request, CancellationToken cancellationToken = default);

    Task<Guid> AcceptInviteAsync(AcceptInviteRequest request, CancellationToken cancellationToken = default);
}
