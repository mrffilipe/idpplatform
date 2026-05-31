using Kyvo.Domain.Entities;

namespace Kyvo.Domain.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
