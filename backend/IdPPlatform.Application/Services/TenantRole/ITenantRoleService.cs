using IdPPlatform.Application.Common;

namespace IdPPlatform.Application.Services.TenantRoles;

public interface ITenantRoleService
{
    Task<Guid> CreateAsync(CreateTenantRoleRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateTenantRoleRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<TenantRoleDto>> ListAsync(
        ListTenantRolesRequest request,
        CancellationToken cancellationToken = default);
}
