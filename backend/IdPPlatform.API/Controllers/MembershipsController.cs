using IdPPlatform.API.Common;
using IdPPlatform.Application.Services.Membership;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Controllers;

[Authorize]
public sealed class MembershipsController : V1ApiControllerBase
{
    private readonly IMembershipService _membershipService;

    public MembershipsController(IMembershipService membershipService)
    {
        _membershipService = membershipService;
    }

    [HttpPost("/v{version:apiVersion}/tenants/{tenantId:guid}/memberships")]
    public async Task<IActionResult> CreateMembership(
        Guid tenantId,
        [FromBody] CreateMembershipBody body,
        CancellationToken cancellationToken)
    {
        var id = await _membershipService.CreateAsync(
            new CreateMembershipRequest
            {
                UserId = body.UserId,
                TenantId = tenantId,
                Roles = body.Roles
            },
            cancellationToken);

        return Ok(new { id });
    }

    [HttpGet("/v{version:apiVersion}/tenants/{tenantId:guid}/memberships")]
    public async Task<IActionResult> ListMembershipsByTenant(
        Guid tenantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _membershipService.ListByTenantAsync(
            new ListMembershipsByTenantRequest
            {
                TenantId = tenantId,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateMembershipRole(
        Guid id,
        [FromBody] UpdateMembershipRoleBody body,
        CancellationToken cancellationToken)
    {
        await _membershipService.UpdateRolesAsync(
            new UpdateMembershipRolesRequest
            {
                MembershipId = id,
                Roles = body.Roles
            },
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RevokeMembership(Guid id, CancellationToken cancellationToken)
    {
        await _membershipService.RevokeAsync(
            new RevokeMembershipRequest { MembershipId = id },
            cancellationToken);

        return NoContent();
    }

    public sealed record CreateMembershipBody(Guid UserId, IReadOnlyCollection<string> Roles);
    public sealed record UpdateMembershipRoleBody(IReadOnlyCollection<string> Roles);
}
