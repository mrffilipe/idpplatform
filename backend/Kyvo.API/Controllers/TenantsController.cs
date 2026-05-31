using Kyvo.API.Common;
using Kyvo.API.Models;
using Kyvo.Application.Common;
using Kyvo.Application.Services.Tenant;
using Kyvo.Application.Services.UserScope;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kyvo.API.Controllers;

/// <summary>
/// Tenant lifecycle, invites, and membership onboarding.
/// </summary>
public sealed class TenantsController : V1ApiControllerBase
{
    private readonly IUserScope _userScope;
    private readonly ITenantService _tenantService;

    public TenantsController(IUserScope userScope, ITenantService tenantService)
    {
        _userScope = userScope;
        _tenantService = tenantService;
    }

    /// <summary>
    /// Creates a new tenant (platform administrators only).
    /// </summary>
    [Authorize(Policy = "PlatformAdministrator")]
    [HttpPost]
    [ProducesResponseType(typeof(CreatedIdResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreatedIdResponse>> CreateTenant(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _tenantService.CreateAsync(
            request with { ActorUserId = _userScope.UserId },
            cancellationToken);

        return CreatedAtAction(nameof(GetTenantById), new { id, version = "1.0" }, new CreatedIdResponse(id));
    }

    /// <summary>
    /// Sends an invitation to join the tenant.
    /// </summary>
    [HttpPost("{id:guid}/invites")]
    [ProducesResponseType(typeof(CreatedIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreatedIdResponse>> InviteMember(
        Guid id,
        [FromBody] InviteMemberRequest request,
        CancellationToken cancellationToken)
    {
        var inviteId = await _tenantService.InviteMemberAsync(
            request with
            {
                TenantId = id,
                InvitedByUserId = _userScope.UserId,
                ActorUserId = _userScope.UserId,
                ActorPlatformRoles = _userScope.PlatformRoles
            },
            cancellationToken);

        return Ok(new CreatedIdResponse(inviteId));
    }

    /// <summary>
    /// Accepts a tenant invite using the token from the invitation email.
    /// </summary>
    [HttpPost("/v{version:apiVersion}/invites/accept")]
    [ProducesResponseType(typeof(CreatedMembershipIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreatedMembershipIdResponse>> AcceptInvite(
        [FromBody] AcceptInviteRequest request,
        CancellationToken cancellationToken)
    {
        var membershipId = await _tenantService.AcceptInviteAsync(
            request with { ActorUserId = _userScope.UserId },
            cancellationToken);

        return Ok(new CreatedMembershipIdResponse(membershipId));
    }

    /// <summary>
    /// Lists tenants the current user belongs to.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TenantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TenantDto>>> ListTenantsByUser(
        [FromQuery] ListTenantsByUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _tenantService.ListByUserAsync(
            request with { UserId = _userScope.UserId },
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Returns a tenant by identifier when the caller has access.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> GetTenantById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _tenantService.GetByIdAsync(
            new GetTenantByIdRequest
            {
                TenantId = id,
                ActorUserId = _userScope.UserId,
                ActorPlatformRoles = _userScope.PlatformRoles
            },
            cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Updates tenant metadata (name).
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTenant(
        Guid id,
        [FromBody] UpdateTenantRequest request,
        CancellationToken cancellationToken)
    {
        await _tenantService.UpdateAsync(
            request with
            {
                TenantId = id,
                ActorUserId = _userScope.UserId,
                ActorPlatformRoles = _userScope.PlatformRoles
            },
            cancellationToken);

        return NoContent();
    }
}
