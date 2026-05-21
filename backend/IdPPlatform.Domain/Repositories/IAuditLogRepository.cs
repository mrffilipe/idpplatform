using IdPPlatform.Domain.Entities;

namespace IdPPlatform.Domain.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
