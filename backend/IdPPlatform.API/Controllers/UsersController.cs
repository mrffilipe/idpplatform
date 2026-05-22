using IdPPlatform.API.Common;
using IdPPlatform.Application.Services.Users;
using IdPPlatform.Application.Services.UserScope;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdPPlatform.API.Controllers;

[Authorize]
public sealed class UsersController : V1ApiControllerBase
{
    private readonly IUserScope _userScope;
    private readonly IUserService _userService;

    public UsersController(IUserScope userScope, IUserService userService)
    {
        _userScope = userScope;
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(
            new GetUserByIdRequest { UserId = _userScope.UserId },
            cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }

    [HttpPatch("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateMeBody body, CancellationToken cancellationToken)
    {
        await _userService.UpdateProfileAsync(
            new UpdateUserProfileRequest
            {
                UserId = _userScope.UserId,
                DisplayName = body.DisplayName,
                PhotoUrl = body.PhotoUrl
            },
            cancellationToken);

        return NoContent();
    }

    [HttpGet("me/memberships")]
    public async Task<IActionResult> ListUserMemberships(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.ListMembershipsAsync(
            new ListUserMembershipsRequest
            {
                UserId = _userScope.UserId,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return Ok(result);
    }

    public sealed record UpdateMeBody
    {
        public required string DisplayName { get; init; }

        public string? PhotoUrl { get; init; }
    }
}
