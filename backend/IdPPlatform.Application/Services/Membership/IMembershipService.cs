using IdPPlatform.Application.Common;

namespace IdPPlatform.Application.Services.Membership;

public interface IMembershipService
{
    Task<Guid> CreateAsync(CreateMembershipRequest request, CancellationToken cancellationToken = default);

    Task UpdateRolesAsync(UpdateMembershipRolesRequest request, CancellationToken cancellationToken = default);

    Task RevokeAsync(RevokeMembershipRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<MembershipDto>> ListByTenantAsync(
        ListMembershipsByTenantRequest request,
        CancellationToken cancellationToken = default);
}
