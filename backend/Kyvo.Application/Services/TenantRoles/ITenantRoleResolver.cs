using Kyvo.Domain.Entities;

namespace Kyvo.Application.Services.TenantRoles;

public interface ITenantRoleResolver
{
    Task<IReadOnlyList<TenantRole>> ResolveActiveRolesAsync(
        Guid tenantId,
        IReadOnlyCollection<string> roleKeys,
        CancellationToken cancellationToken = default);
}
