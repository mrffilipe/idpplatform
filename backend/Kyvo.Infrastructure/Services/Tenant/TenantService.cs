using Kyvo.Application.Common;
using Kyvo.Application.Exceptions;
using Kyvo.Application.Interfaces;
using Kyvo.Application.Services.Email;
using Kyvo.Application.Services.RefreshTokenHasher;
using Kyvo.Application.Services.Tenant;
using Kyvo.Application.Services.TenantResolutionCache;
using Kyvo.Application.Services.TenantRoles;
using Kyvo.Application.Services.UnitOfWork;
using Kyvo.Domain.Constants;
using Kyvo.Domain.Entities;
using Kyvo.Domain.Exceptions;
using Kyvo.Domain.Repositories;
using Kyvo.Domain.ValueObjects;
using Kyvo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kyvo.Infrastructure.Services.Tenant;

public sealed class TenantService : ITenantService
{
    private readonly ITenantRepository _tenants;
    private readonly ITenantRoleRepository _roles;
    private readonly ITenantMembershipRepository _memberships;
    private readonly ITenantInviteRepository _invites;
    private readonly IUserRepository _users;
    private readonly IRefreshTokenHasher _hasher;
    private readonly IInvitePolicy _policy;
    private readonly ITenantRoleResolver _roleResolver;
    private readonly IEmailService _emailService;
    private readonly ITenantResolutionCache _tenantResolutionCache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public TenantService(
        ITenantRepository tenants,
        ITenantRoleRepository roles,
        ITenantMembershipRepository memberships,
        ITenantInviteRepository invites,
        IUserRepository users,
        IRefreshTokenHasher hasher,
        IInvitePolicy policy,
        ITenantRoleResolver roleResolver,
        IEmailService emailService,
        ITenantResolutionCache tenantResolutionCache,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _tenants = tenants;
        _roles = roles;
        _memberships = memberships;
        _invites = invites;
        _users = users;
        _hasher = hasher;
        _policy = policy;
        _roleResolver = roleResolver;
        _emailService = emailService;
        _tenantResolutionCache = tenantResolutionCache;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Guid> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken = default)
    {
        var key = new TenantKey(request.Key);
        if (await _tenants.KeyAlreadyExistsAsync(key, cancellationToken))
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.Tenant.KeyAlreadyExists);
        }

        var initialAdministratorUserId = request.InitialAdministratorUserId ?? request.ActorUserId;
        var initialAdministrator = await _users.GetForUpdateAsync(initialAdministratorUserId, cancellationToken)
            ?? throw new DomainNotFoundException(DomainErrorMessages.User.UserNotFound);

        if (!initialAdministrator.IsActive)
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.User.UserInactive);
        }

        var tenant = new Domain.Entities.Tenant(request.Name, key);
        await _tenants.AddAsync(tenant, cancellationToken);

        Domain.Entities.TenantRole? ownerRole = null;
        foreach (var role in TenantRoleDefaults.All)
        {
            var createdRole = new Domain.Entities.TenantRole(
                tenant.Id,
                role.Key,
                role.Name,
                isSystem: true);
            await _roles.AddAsync(createdRole, cancellationToken);

            if (role.Key.Equals(TenantRoleDefaults.Owner, StringComparison.OrdinalIgnoreCase))
            {
                ownerRole = createdRole;
            }
        }

        if (ownerRole is null)
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.TenantRole.AtLeastOneRoleRequired);
        }

        await _memberships.AddAsync(
            new TenantMembership(tenant.Id, initialAdministratorUserId, [ownerRole]),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _tenantResolutionCache.InvalidateByIdentifierAsync(key.Value, cancellationToken);
        return tenant.Id;
    }

    public async Task UpdateAsync(UpdateTenantRequest request, CancellationToken cancellationToken = default)
    {
        var isPlatformAdministrator = request.ActorPlatformRoles
            .Any(role => PlatformRoleDefaults.AdministrativeKeys.Contains(role));

        if (!isPlatformAdministrator)
        {
            var membership = await _memberships.GetByUserIdAndTenantIdWithRolesAsync(
                request.ActorUserId,
                request.TenantId,
                cancellationToken);

            var hasAdministrativeRole = membership is not null
                && membership.IsActive
                && membership.Roles.Any(role => TenantRoleDefaults.AdministrativeKeys.Contains(role.Role.Key.Value));

            if (!hasAdministrativeRole)
            {
                throw new ForbiddenApplicationException(ApplicationErrorMessages.Auth.UserHasNoTenantAccess);
            }
        }

        var tenant = await _tenants.GetForUpdateAsync(request.TenantId, cancellationToken)
            ?? throw new DomainNotFoundException(DomainErrorMessages.Tenant.TenantNotFound);

        tenant.UpdateName(request.Name);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _tenantResolutionCache.InvalidateByIdentifierAsync(tenant.Key.Value, cancellationToken);
    }

    public async Task<TenantDto?> GetByIdAsync(GetTenantByIdRequest request, CancellationToken cancellationToken = default)
    {
        var isPlatformAdministrator = request.ActorPlatformRoles
            .Any(role => PlatformRoleDefaults.AdministrativeKeys.Contains(role));

        if (!isPlatformAdministrator)
        {
            var hasAdministrativeMembership = await _context.TenantMemberships
                .AsNoTracking()
                .Where(x => x.UserId == request.ActorUserId && x.TenantId == request.TenantId && x.IsActive)
                .AnyAsync(
                    membership => membership.Roles.Any(
                        role => TenantRoleDefaults.AdministrativeKeys.Contains(role.Role.Key.Value)),
                    cancellationToken);

            if (!hasAdministrativeMembership)
            {
                return null;
            }
        }

        return await _context.Tenants
            .AsNoTracking()
            .Where(x => x.Id == request.TenantId)
            .Select(x => new TenantDto
            {
                Id = x.Id,
                Name = x.Name,
                Key = x.Key.Value
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<TenantDto>> ListByUserAsync(
        ListTenantsByUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var query = _context.TenantMemberships
            .AsNoTracking()
            .Include(x => x.Tenant)
            .Where(x => x.UserId == request.UserId && x.IsActive);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Tenant.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TenantDto
            {
                Id = x.TenantId,
                Name = x.Tenant.Name,
                Key = x.Tenant.Key.Value
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TenantDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Guid> InviteMemberAsync(InviteMemberRequest request, CancellationToken cancellationToken = default)
    {
        var isPlatformAdministrator = request.ActorPlatformRoles
            .Any(role => PlatformRoleDefaults.AdministrativeKeys.Contains(role));

        if (!isPlatformAdministrator)
        {
            var membership = await _memberships.GetByUserIdAndTenantIdWithRolesAsync(
                request.ActorUserId,
                request.TenantId,
                cancellationToken);

            var hasAdministrativeRole = membership is not null
                && membership.IsActive
                && membership.Roles.Any(role => TenantRoleDefaults.AdministrativeKeys.Contains(role.Role.Key.Value));

            if (!hasAdministrativeRole)
            {
                throw new ForbiddenApplicationException(ApplicationErrorMessages.Auth.UserHasNoTenantAccess);
            }
        }

        var tenant = await _tenants.GetForUpdateAsync(request.TenantId, cancellationToken)
            ?? throw new DomainNotFoundException(DomainErrorMessages.Tenant.TenantNotFound);

        var roles = await _roleResolver.ResolveActiveRolesAsync(
            request.TenantId,
            request.Roles,
            cancellationToken);

        var rawToken = GenerateToken();
        var invite = new TenantInvite(
            request.TenantId,
            request.Email,
            roles,
            _hasher.Hash(rawToken),
            DateTime.UtcNow.AddHours(_policy.ExpirationHours),
            request.InvitedByUserId);

        await _invites.AddAsync(invite, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _emailService.SendInviteAsync(invite.Email.Value, tenant.Name, rawToken, cancellationToken);

        return invite.Id;
    }

    public async Task<Guid> AcceptInviteAsync(AcceptInviteRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = _hasher.Hash(request.InviteToken);
        var invite = await _invites.GetByTokenHashWithRolesAsync(tokenHash, cancellationToken)
            ?? throw new DomainNotFoundException(DomainErrorMessages.TenantInvite.InviteNotFound);

        if (invite.IsConsumed())
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.TenantInvite.AlreadyConsumed);
        }

        if (invite.IsExpired())
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.TenantInvite.Expired);
        }

        var user = await _users.GetForUpdateAsync(request.ActorUserId, cancellationToken)
            ?? throw new DomainNotFoundException(DomainErrorMessages.User.UserNotFound);

        if (!string.Equals(user.Email.Value, invite.Email.Value, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.TenantInvite.EmailMismatch);
        }

        var membership = await _memberships.GetByUserIdAndTenantIdWithRolesAsync(
            user.Id,
            invite.TenantId,
            cancellationToken);

        if (membership is null || !membership.IsActive)
        {
            membership = new TenantMembership(
                invite.TenantId,
                user.Id,
                invite.Roles.Select(x => x.Role));
            await _memberships.AddAsync(membership, cancellationToken);
        }
        else
        {
            membership.MergeRoles(invite.Roles.Select(x => x.Role));
        }

        invite.Consume();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return membership.Id;
    }

    private static string GenerateToken()
    {
        var bytes = new byte[64];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
