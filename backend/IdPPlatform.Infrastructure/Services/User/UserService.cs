using IdPPlatform.Application.Common;
using IdPPlatform.Application.Services.Users;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Exceptions;
using IdPPlatform.Domain.Repositories;
using IdPPlatform.Domain.ValueObjects;
using IdPPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdPPlatform.Infrastructure.Services.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IExternalIdentityRepository _externalIdentities;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public UserService(
        IUserRepository users,
        IExternalIdentityRepository externalIdentities,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _users = users;
        _externalIdentities = externalIdentities;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Guid> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var email = new EmailAddress(request.Email);
        if (await _users.EmailAlreadyExistsAsync(email, cancellationToken))
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.User.EmailAlreadyExists);
        }

        var user = new Domain.Entities.User(email, request.DisplayName, request.PhotoUrl);
        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user.Id;
    }

    public async Task UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetForUpdateAsync(request.UserId, cancellationToken)
            ?? throw new DomainNotFoundException(DomainErrorMessages.User.UserNotFound);

        user.UpdateProfile(request.DisplayName, request.PhotoUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto?> GetByIdAsync(GetUserByIdRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(x => x.Memberships)
            .ThenInclude(x => x.Tenant)
            .Include(x => x.Memberships)
            .ThenInclude(x => x.Roles)
            .ThenInclude(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PhotoUrl = user.PhotoUrl,
            Memberships = user.Memberships
                .Where(x => x.IsActive)
                .Select(x => new UserMembershipDto
                {
                    MembershipId = x.Id,
                    TenantId = x.TenantId,
                    TenantName = x.Tenant.Name,
                    TenantKey = x.Tenant.Key.Value,
                    Roles = x.Roles.Select(role => role.Role.Key.Value).ToList()
                })
                .ToList()
        };
    }

    public async Task<PagedResult<UserMembershipDto>> ListMembershipsAsync(
        ListUserMembershipsRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var query = _context.TenantMemberships
            .AsNoTracking()
            .Include(x => x.Tenant)
            .Include(x => x.Roles)
            .ThenInclude(x => x.Role)
            .Where(x => x.UserId == request.UserId && x.IsActive);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Tenant.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserMembershipDto
            {
                MembershipId = x.Id,
                TenantId = x.TenantId,
                TenantName = x.Tenant.Name,
                TenantKey = x.Tenant.Key.Value,
                Roles = x.Roles.Select(role => role.Role.Key.Value).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<UserMembershipDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task LinkExternalIdentityAsync(
        LinkExternalIdentityRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _externalIdentities.GetByProviderAndProviderUserIdAsync(
            request.Provider,
            request.ProviderUserId,
            cancellationToken);

        if (existing is not null)
        {
            return;
        }

        var externalIdentity = new Domain.Entities.ExternalIdentity(
            request.UserId,
            request.Provider,
            request.ProviderUserId,
            request.Email);
        await _externalIdentities.AddAsync(externalIdentity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
