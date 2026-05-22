using IdPPlatform.API.Common;
using IdPPlatform.Application.Services.IdentityProvider;
using IdPPlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Controllers;

[Authorize(Policy = "PlatformAdministrator")]
public sealed class IdentityProvidersController : V1ApiControllerBase
{
    private readonly IIdentityProviderService _identityProviderService;

    public IdentityProvidersController(IIdentityProviderService identityProviderService)
    {
        _identityProviderService = identityProviderService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _identityProviderService.ListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _identityProviderService.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddIdentityProviderBody body, CancellationToken cancellationToken)
    {
        var result = await _identityProviderService.AddAsync(
            new AddIdentityProviderRequest
            {
                Alias = body.Alias,
                DisplayName = body.DisplayName,
                ProviderType = body.ProviderType,
                Capabilities = body.Capabilities,
                ConfigJson = body.ConfigJson
            },
            cancellationToken);

        return Ok(new { id = result.Id, warnings = result.Warnings });
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateIdentityProviderBody body,
        CancellationToken cancellationToken)
    {
        await _identityProviderService.UpdateAsync(
            new UpdateIdentityProviderRequest
            {
                Id = id,
                DisplayName = body.DisplayName,
                Capabilities = body.Capabilities,
                ConfigJson = body.ConfigJson
            },
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/enable")]
    public async Task<IActionResult> Enable(Guid id, CancellationToken cancellationToken)
    {
        await _identityProviderService.EnableAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/disable")]
    public async Task<IActionResult> Disable(Guid id, CancellationToken cancellationToken)
    {
        await _identityProviderService.DisableAsync(id, cancellationToken);
        return NoContent();
    }

    public sealed record AddIdentityProviderBody
    {
        public required string Alias { get; init; }

        public required string DisplayName { get; init; }

        public required IdentityProviderType ProviderType { get; init; }

        public required IReadOnlyCollection<IdpCapability> Capabilities { get; init; }

        public string? ConfigJson { get; init; }
    }

    public sealed record UpdateIdentityProviderBody
    {
        public required string DisplayName { get; init; }

        public IReadOnlyCollection<IdpCapability>? Capabilities { get; init; }

        public string? ConfigJson { get; init; }
    }
}
