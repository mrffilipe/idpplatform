using IdPPlatform.API.Common;
using IdPPlatform.Application.Services.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IdPPlatform.API.Controllers;

[Route("v{version:apiVersion}/platform")]
public sealed class PlatformController : V1ApiControllerBase
{
    private readonly IPlatformService _platformService;

    public PlatformController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var result = await _platformService.GetStatusAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("bootstrap")]
    [AllowAnonymous]
    [EnableRateLimiting("platform_bootstrap")]
    public async Task<IActionResult> Bootstrap(CancellationToken cancellationToken)
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
