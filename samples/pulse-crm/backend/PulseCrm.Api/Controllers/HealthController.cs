using Microsoft.AspNetCore.Mvc;

namespace PulseCrm.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
        Ok(new { status = "ok", service = "PulseCRM.Api", timestamp = DateTime.UtcNow });
}
