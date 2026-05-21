using IdPPlatform.API.Common;
using IdPPlatform.Application.Services.Tenant;
using IdPPlatform.Application.Services.UserScope;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Controllers;

[Authorize]
public sealed class TenantsController : V1ApiControllerBase
{
    private readonly IUserScope _userScope;
    private readonly ITenantService _tenantService;

    public TenantsController(IUserScope userScope, ITenantService tenantService)
    {
        _userScope = userScope;
        _tenantService = tenantService;
    }

    [Authorize(Policy = "PlatformAdministrator")]
    [HttpPost]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantBody body, CancellationToken cancellationToken)
    {
        var id = await _tenantService.CreateAsync(
            new CreateTenantRequest
            {
                Name = body.Name,
                Key = body.Key,
                ActorUserId = _userScope.UserId,
                InitialAdministratorUserId = body.InitialAdministratorUserId
            },
            cancellationToken);

        return CreatedAtAction(nameof(GetTenantById), new { id, version = "1.0" }, new { id });
    }

    [HttpGet]
    public async Task<IActionResult> ListTenantsByUser(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _tenantService.ListByUserAsync(
            new ListTenantsByUserRequest
            {
                UserId = _userScope.UserId,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTenantById(Guid id, CancellationToken cancellationToken)
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

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdateTenant(
        Guid id,
        [FromBody] UpdateTenantBody body,
        CancellationToken cancellationToken)
    {
        await _tenantService.UpdateAsync(
            new UpdateTenantRequest
            {
                TenantId = id,
                Name = body.Name,
                ActorUserId = _userScope.UserId,
                ActorPlatformRoles = _userScope.PlatformRoles
            },
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/invites")]
    public async Task<IActionResult> InviteMember(
        Guid id,
        [FromBody] InviteMemberBody body,
        CancellationToken cancellationToken)
    {
        var inviteId = await _tenantService.InviteMemberAsync(
            new InviteMemberRequest
            {
                TenantId = id,
                Email = body.Email,
                Roles = body.Roles,
                InvitedByUserId = _userScope.UserId,
                ActorUserId = _userScope.UserId,
                ActorPlatformRoles = _userScope.PlatformRoles
            },
            cancellationToken);

        return Ok(new { id = inviteId });
    }

    [HttpPost("/v{version:apiVersion}/invites/accept")]
    public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteBody body, CancellationToken cancellationToken)
    {
        var membershipId = await _tenantService.AcceptInviteAsync(
            new AcceptInviteRequest
            {
                InviteToken = body.Token,
                ActorUserId = _userScope.UserId
            },
            cancellationToken);

        return Ok(new { membershipId });
    }

    public sealed record CreateTenantBody(string Name, string Key, Guid? InitialAdministratorUserId);
    public sealed record UpdateTenantBody(string Name);
    public sealed record InviteMemberBody(string Email, IReadOnlyCollection<string> Roles);
    public sealed record AcceptInviteBody(string Token);
}
