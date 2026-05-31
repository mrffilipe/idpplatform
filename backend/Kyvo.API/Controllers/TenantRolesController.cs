using Kyvo.API.Common;
using Kyvo.API.Models;
using Kyvo.Application.Common;
using Kyvo.Application.Services.TenantRoles;
using Microsoft.AspNetCore.Mvc;

namespace Kyvo.API.Controllers;

/// <summary>
/// Custom tenant role definitions (RBAC keys within a tenant).
/// </summary>
public sealed class TenantRolesController : V1ApiControllerBase
{
    private readonly ITenantRoleService _tenantRoleService;

    public TenantRolesController(ITenantRoleService tenantRoleService) => _tenantRoleService = tenantRoleService;

    /// <summary>
    /// Creates a custom role for the tenant.
    /// </summary>
    [HttpPost("/v{version:apiVersion}/tenants/{tenantId:guid}/roles")]
    [ProducesResponseType(typeof(CreatedIdResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreatedIdResponse>> CreateTenantRole(
        Guid tenantId,
        [FromBody] CreateTenantRoleRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _tenantRoleService.CreateAsync(
            request with { TenantId = tenantId },
            cancellationToken);

        return Ok(new CreatedIdResponse(id));
    }

    /// <summary>
    /// Lists roles defined for a tenant.
    /// </summary>
    [HttpGet("/v{version:apiVersion}/tenants/{tenantId:guid}/roles")]
    [ProducesResponseType(typeof(PagedResult<TenantRoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TenantRoleDto>>> ListTenantRoles(
        Guid tenantId,
        [FromQuery] ListTenantRolesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _tenantRoleService.ListAsync(
            request with { TenantId = tenantId },
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Updates a tenant role's name, description, or active flag.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTenantRole(
        Guid id,
        [FromBody] UpdateTenantRoleRequest request,
        CancellationToken cancellationToken)
    {
        await _tenantRoleService.UpdateAsync(
            request with { RoleId = id },
            cancellationToken);

        return NoContent();
    }
}
