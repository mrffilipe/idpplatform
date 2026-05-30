using IdPPlatform.API.Common;
using IdPPlatform.API.Models;
using IdPPlatform.Application.Common;
using IdPPlatform.Application.Services.AppService;
using IdPPlatform.Application.Services.UserScope;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Controllers;

/// <summary>
/// Manages SaaS applications, OAuth clients, and tenant provisioning for an application.
/// </summary>
public sealed class ApplicationsController : V1ApiControllerBase
{
    private readonly IUserScope _userScope;
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IUserScope userScope, IApplicationService applicationService)
    {
        _userScope = userScope;
        _applicationService = applicationService;
    }

    /// <summary>
    /// Registers a new application (platform administrators only).
    /// </summary>
    [Authorize(Policy = "PlatformAdministrator")]
    [HttpPost]
    [ProducesResponseType(typeof(CreatedIdResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreatedIdResponse>> CreateApplication(
        [FromBody] CreateApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _applicationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetApplicationById), new { id, version = "1.0" }, new CreatedIdResponse(id));
    }

    /// <summary>
    /// Creates an OAuth/OIDC client for the given application.
    /// </summary>
    [HttpPost("{applicationId:guid}/clients")]
    [ProducesResponseType(typeof(CreatedIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreatedIdResponse>> CreateApplicationClient(
        Guid applicationId,
        [FromBody] CreateApplicationClientRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _applicationService.CreateClientAsync(
            request with
            {
                ApplicationId = applicationId,
                ActorPlatformRoles = _userScope.PlatformRoles
            },
            cancellationToken);

        return Ok(new CreatedIdResponse(id));
    }

    /// <summary>
    /// Provisions a tenant linked to an application (platform administrators only).
    /// </summary>
    [Authorize(Policy = "PlatformAdministrator")]
    [HttpPost("{applicationId:guid}/tenants/provision")]
    [ProducesResponseType(typeof(ProvisionApplicationTenantResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProvisionApplicationTenantResult>> ProvisionTenant(
        Guid applicationId,
        [FromBody] ProvisionApplicationTenantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _applicationService.ProvisionTenantAsync(
            request with
            {
                ApplicationId = applicationId,
                ActorUserId = _userScope.UserId,
                ActorPlatformRoles = _userScope.PlatformRoles
            },
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lists applications with pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ApplicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ApplicationDto>>> ListApplications(
        [FromQuery] ListApplicationsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _applicationService.ListAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns a single application by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationDto>> GetApplicationById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _applicationService.GetByIdAsync(
            new GetApplicationByIdRequest { ApplicationId = id },
            cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }
}
