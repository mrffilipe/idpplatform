using Kyvo.Domain.Entities;
using Kyvo.Domain.Repositories;

namespace Kyvo.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        return _context.AuditLogs
            .AddAsync(auditLog, cancellationToken)
            .AsTask();
    }
}
