using Kyvo.API.Common;
using Kyvo.Application.Common;
using Kyvo.Application.Services.Users;
using Kyvo.Application.Services.UserScope;
using Microsoft.AspNetCore.Mvc;

namespace Kyvo.API.Controllers;

/// <summary>
/// Current user profile and membership listing.
/// </summary>
public sealed class UsersController : V1ApiControllerBase
{
    private readonly IUserScope _userScope;
    private readonly IUserService _userService;

    public UsersController(IUserScope userScope, IUserService userService)
    {
        _userScope = userScope;
        _userService = userService;
    }

    /// <summary>
    /// Returns the authenticated user's profile.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(
            new GetUserByIdRequest { UserId = _userScope.UserId },
            cancellationToken);

        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Lists tenants and roles for the authenticated user.
    /// </summary>
    [HttpGet("me/memberships")]
    [ProducesResponseType(typeof(PagedResult<UserMembershipDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserMembershipDto>>> ListUserMemberships(
        [FromQuery] ListUserMembershipsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _userService.ListMembershipsAsync(
            request with { UserId = _userScope.UserId },
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Updates the authenticated user's display name and photo URL.
    /// </summary>
    [HttpPatch("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        await _userService.UpdateProfileAsync(
            request with { UserId = _userScope.UserId },
            cancellationToken);

        return NoContent();
    }
}
