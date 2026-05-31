using Kyvo.API.Common;
using Kyvo.Application.Services.IdentityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kyvo.API.Controllers;

/// <summary>
/// Platform-wide identity provider configuration (Firebase, local password, etc.).
/// </summary>
[Authorize(Policy = "PlatformAdministrator")]
public sealed class IdentityProvidersController : V1ApiControllerBase
{
    private readonly IIdentityProviderService _identityProviderService;

    public IdentityProvidersController(IIdentityProviderService identityProviderService) =>
        _identityProviderService = identityProviderService;

    /// <summary>
    /// Registers a new identity provider.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AddIdentityProviderResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<AddIdentityProviderResult>> Add(
        [FromBody] AddIdentityProviderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _identityProviderService.AddAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lists all identity providers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<IdentityProviderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<IdentityProviderDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _identityProviderService.ListAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns a single identity provider by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IdentityProviderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IdentityProviderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _identityProviderService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Updates display name, capabilities, or encrypted configuration.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateIdentityProviderRequest request,
        CancellationToken cancellationToken)
    {
        await _identityProviderService.UpdateAsync(
            request with { Id = id },
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Enables an identity provider for login flows.
    /// </summary>
    [HttpPost("{id:guid}/enable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Enable(Guid id, CancellationToken cancellationToken)
    {
        await _identityProviderService.EnableAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Disables an identity provider.
    /// </summary>
    [HttpPost("{id:guid}/disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disable(Guid id, CancellationToken cancellationToken)
    {
        await _identityProviderService.DisableAsync(id, cancellationToken);
        return NoContent();
    }
}
