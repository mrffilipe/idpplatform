using Kyvo.API.Common;
using Kyvo.Application.Services.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Kyvo.API.Controllers;

/// <summary>
/// Platform lifecycle: bootstrap and configuration status.
/// </summary>
public sealed class PlatformController : V1ApiControllerBase
{
    private readonly IPlatformService _platformService;

    public PlatformController(IPlatformService platformService) => _platformService = platformService;

    /// <summary>
    /// Returns whether the platform has been bootstrapped and which OAuth client to use for admin login.
    /// </summary>
    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PlatformStatusResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlatformStatusResult>> GetStatus(CancellationToken cancellationToken)
    {
        var result = await _platformService.GetStatusAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Performs one-time platform bootstrap (root user and default OAuth client). Allowed only while unconfigured.
    /// </summary>
    [HttpPost("bootstrap")]
    [AllowAnonymous]
    [EnableRateLimiting("platform_bootstrap")]
    [ProducesResponseType(typeof(BootstrapResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BootstrapResult>> Bootstrap(CancellationToken cancellationToken)
    {
        var result = await _platformService.BootstrapAsync(
            new BootstrapRequest
            {
                UserAgent = Request.Headers.UserAgent.ToString(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            },
            cancellationToken);

        return Ok(result);
    }
}
