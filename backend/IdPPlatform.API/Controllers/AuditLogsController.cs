using IdPPlatform.API.Common;
using IdPPlatform.Application.Common;
using IdPPlatform.Application.Services.AuditLog;
using IdPPlatform.Application.Services.UserScope;
using IdPPlatform.Domain.Constants;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Controllers;

/// <summary>
/// Tenant-scoped audit log queries (tenant owners and administrators).
/// </summary>
public sealed class AuditLogsController : V1ApiControllerBase
{
    private readonly IUserScope _userScope;
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IUserScope userScope, IAuditLogService auditLogService)
    {
        _userScope = userScope;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Lists audit log entries for the current tenant with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<AuditLogItemDto>>> ListAuditLogs(
        [FromQuery] ListAuditLogsRequest request,
        CancellationToken cancellationToken)
    {
        if (!_userScope.HasAnyTenantRole(TenantRoleDefaults.Owner, TenantRoleDefaults.Admin))
        {
            return Forbid();
        }

        var result = await _auditLogService.ListAsync(request, cancellationToken);
        return Ok(result);
    }
}
