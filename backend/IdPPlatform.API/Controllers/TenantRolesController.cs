using IdPPlatform.API.Common;
using IdPPlatform.Application.Services.TenantRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Controllers;

[Authorize]
public sealed class TenantRolesController : V1ApiControllerBase
{
    private readonly ITenantRoleService _tenantRoleService;

    public TenantRolesController(ITenantRoleService tenantRoleService)
    {
        _tenantRoleService = tenantRoleService;
    }

    [HttpGet("/v{version:apiVersion}/tenants/{tenantId:guid}/roles")]
    public async Task<IActionResult> ListTenantRoles(
        Guid tenantId,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _tenantRoleService.ListAsync(
            new ListTenantRolesRequest
            {
                TenantId = tenantId,
                IncludeInactive = includeInactive,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("/v{version:apiVersion}/tenants/{tenantId:guid}/roles")]
    public async Task<IActionResult> CreateTenantRole(
        Guid tenantId,
        [FromBody] CreateTenantRoleBody body,
        CancellationToken cancellationToken)
    {
        var id = await _tenantRoleService.CreateAsync(
            new CreateTenantRoleRequest
            {
                TenantId = tenantId,
                Key = body.Key,
                Name = body.Name,
                Description = body.Description
            },
            cancellationToken);

        return Ok(new { id });
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateTenantRole(
        Guid id,
        [FromBody] UpdateTenantRoleBody body,
        CancellationToken cancellationToken)
    {
        await _tenantRoleService.UpdateAsync(
            new UpdateTenantRoleRequest
            {
                RoleId = id,
                Name = body.Name,
                Description = body.Description,
                IsActive = body.IsActive
            },
            cancellationToken);

        return NoContent();
    }

    public sealed record CreateTenantRoleBody(string Key, string Name, string? Description);
    public sealed record UpdateTenantRoleBody(string Name, string? Description, bool IsActive);
}
