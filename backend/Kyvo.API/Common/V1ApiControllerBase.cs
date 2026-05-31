using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kyvo.API.Common;

/// <summary>
/// Base for versioned JSON API controllers under <c>/v{version}/...</c> with JWT bearer authentication.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public abstract class V1ApiControllerBase : ControllerBase
{
}
