using IdPPlatform.API.Common;
using IdPPlatform.Application.Services.AppService;
using IdPPlatform.Application.Services.UserScope;
using IdPPlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Controllers;

[Authorize]
public sealed class ApplicationsController : V1ApiControllerBase
{
    private readonly IUserScope _userScope;
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IUserScope userScope, IApplicationService applicationService)
    {
        _userScope = userScope;
        _applicationService = applicationService;
    }

    [Authorize(Policy = "PlatformAdministrator")]
    [HttpPost]
    public async Task<IActionResult> CreateApplication(
        [FromBody] CreateApplicationBody body,
        CancellationToken cancellationToken)
    {
        var id = await _applicationService.CreateAsync(
            new CreateApplicationRequest
            {
                Name = body.Name,
                Slug = body.Slug,
                Type = body.Type
            },
            cancellationToken);

        return CreatedAtAction(nameof(GetApplicationById), new { id, version = "1.0" }, new { id });
    }

    [HttpGet]
    public async Task<IActionResult> ListApplications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _applicationService.ListAsync(
            new ListApplicationsRequest { Page = page, PageSize = pageSize },
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetApplicationById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _applicationService.GetByIdAsync(
            new GetApplicationByIdRequest { ApplicationId = id },
            cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{applicationId:guid}/clients")]
    public async Task<IActionResult> CreateApplicationClient(
        Guid applicationId,
        [FromBody] CreateApplicationClientBody body,
        CancellationToken cancellationToken)
    {
        var id = await _applicationService.CreateClientAsync(
            new CreateApplicationClientRequest
            {
                ApplicationId = applicationId,
                ClientId = body.ClientId,
                ClientSecretHash = body.ClientSecretHash,
                ClientType = body.ClientType,
                RedirectUris = body.RedirectUris,
                AllowedScopes = body.AllowedScopes,
                AccessTokenTtlSeconds = body.AccessTokenTtlSeconds,
                ActorPlatformRoles = _userScope.PlatformRoles
            },
            cancellationToken);

        return Ok(new { id });
    }

    [Authorize(Policy = "PlatformAdministrator")]
    [HttpPost("{applicationId:guid}/tenants/provision")]
    public async Task<IActionResult> ProvisionTenant(
        Guid applicationId,
        [FromBody] ProvisionApplicationTenantBody body,
        CancellationToken cancellationToken)
    {
        var result = await _applicationService.ProvisionTenantAsync(
            new ProvisionApplicationTenantRequest
            {
                ApplicationId = applicationId,
                TenantName = body.TenantName,
                TenantKey = body.TenantKey,
                InitialAdministratorUserId = body.InitialAdministratorUserId,
                ExternalCustomerId = body.ExternalCustomerId,
                PlanCode = body.PlanCode,
                ActorUserId = _userScope.UserId,
                ActorPlatformRoles = _userScope.PlatformRoles
            },
            cancellationToken);

        return Ok(result);
    }

    public sealed record CreateApplicationBody
    {
        public required string Name { get; init; }

        public required string Slug { get; init; }

        public required ApplicationType Type { get; init; }
    }

    public sealed record CreateApplicationClientBody
    {
        public required string ClientId { get; init; }

        public string? ClientSecretHash { get; init; }

        public required ClientType ClientType { get; init; }

        public required string RedirectUris { get; init; }

        public required string AllowedScopes { get; init; }

        public required int AccessTokenTtlSeconds { get; init; }
    }

    public sealed record ProvisionApplicationTenantBody
    {
        public required string TenantName { get; init; }

        public required string TenantKey { get; init; }

        public Guid? InitialAdministratorUserId { get; init; }

        public string? ExternalCustomerId { get; init; }

        public string? PlanCode { get; init; }
    }
}
