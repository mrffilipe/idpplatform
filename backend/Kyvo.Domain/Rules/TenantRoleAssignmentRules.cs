using Kyvo.Domain.Entities;
using Kyvo.Domain.Exceptions;

namespace Kyvo.Domain.Rules;

public static class TenantRoleAssignmentRules
{
    public static IReadOnlyList<TenantRole> ValidateForTenant(Guid tenantId, IEnumerable<TenantRole>? roles)
    {
        var normalizedRoles = roles?.ToList() ?? [];
        if (normalizedRoles.Count == 0)
        {
            throw new DomainValidationException(DomainErrorMessages.TenantRole.AtLeastOneRoleRequired);
        }

        if (normalizedRoles.Any(x => x.TenantId != tenantId))
        {
            throw new DomainValidationException(DomainErrorMessages.TenantRole.RoleTenantMismatch);
        }

        if (normalizedRoles.Any(x => !x.IsActive))
        {
            throw new DomainValidationException(DomainErrorMessages.TenantRole.InactiveRole);
        }

        var duplicates = normalizedRoles
            .GroupBy(x => x.Id)
            .Any(x => x.Count() > 1);
        if (duplicates)
        {
            throw new DomainValidationException(DomainErrorMessages.TenantRole.DuplicateRole);
        }

        return normalizedRoles;
    }
}
