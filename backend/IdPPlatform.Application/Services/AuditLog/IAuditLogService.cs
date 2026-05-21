using IdPPlatform.Application.Common;

namespace IdPPlatform.Application.Services.AuditLog;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogItemDto>> ListAsync(
        ListAuditLogsRequest request,
        CancellationToken cancellationToken = default);
}
