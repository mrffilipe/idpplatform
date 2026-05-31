using Kyvo.Domain.Entities;
using Kyvo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kyvo.Infrastructure.Persistence.Repositories;

public sealed class ApplicationTenantRepository : IApplicationTenantRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicationTenantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(ApplicationTenant applicationTenant, CancellationToken cancellationToken = default)
    {
        return _context.ApplicationTenants
            .AddAsync(applicationTenant, cancellationToken)
            .AsTask();
    }

    public Task<bool> ExistsAsync(
        Guid applicationId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return _context.ApplicationTenants
            .AnyAsync(x => x.ApplicationId == applicationId && x.TenantId == tenantId, cancellationToken);
    }
}
