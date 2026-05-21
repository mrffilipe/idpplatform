using IdPPlatform.API.Common;
using IdPPlatform.Application.Services.AuditLog;
using IdPPlatform.Application.Services.UserScope;
using IdPPlatform.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Controllers;

[Authorize]
public sealed class AuditLogsController : V1ApiControllerBase
{
    private readonly IUserScope _userScope;
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IUserScope userScope, IAuditLogService auditLogService)
    {
        _userScope = userScope;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> ListAuditLogs(
        [FromQuery] Guid? userId,
        [FromQuery] string? action,
        [FromQuery] string? resourceType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!_userScope.HasAnyTenantRole(TenantRoleDefaults.Owner, TenantRoleDefaults.Admin))
        {
            return Forbid();
        }

        var result = await _auditLogService.ListAsync(
            new ListAuditLogsRequest
            {
                UserId = userId,
                Action = action,
                ResourceType = resourceType,
                From = from,
                To = to,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return Ok(result);
    }
}
