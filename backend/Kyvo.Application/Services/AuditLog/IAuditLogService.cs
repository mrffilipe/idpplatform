using Kyvo.Application.Common;

namespace Kyvo.Application.Services.AuditLog;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogItemDto>> ListAsync(
        ListAuditLogsRequest request,
        CancellationToken cancellationToken = default);
}
