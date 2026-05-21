using IdPPlatform.Domain.Entities;

namespace IdPPlatform.Application.Services.TenantRoles;

public interface ITenantRoleResolver
{
    Task<IReadOnlyList<TenantRole>> ResolveActiveRolesAsync(
        Guid tenantId,
        IReadOnlyCollection<string> roleKeys,
        CancellationToken cancellationToken = default);
}
