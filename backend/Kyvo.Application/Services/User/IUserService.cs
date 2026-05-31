using Kyvo.Application.Common;

namespace Kyvo.Application.Services.Users;

public interface IUserService
{
    Task<Guid> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default);

    Task<UserDto?> GetByIdAsync(GetUserByIdRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<UserMembershipDto>> ListMembershipsAsync(
        ListUserMembershipsRequest request,
        CancellationToken cancellationToken = default);

    Task LinkExternalIdentityAsync(
        LinkExternalIdentityRequest request,
        CancellationToken cancellationToken = default);
}
