using IdPPlatform.Application.Common;
using IdPPlatform.Application.Services.Membership;
using IdPPlatform.Application.Services.TenantRoles;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Exceptions;
using IdPPlatform.Domain.Repositories;
using IdPPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdPPlatform.Infrastructure.Services.Membership;

public sealed class MembershipService : IMembershipService
{
    private readonly ITenantMembershipRepository _memberships;
    private readonly ITenantRoleResolver _roleResolver;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public MembershipService(
        ITenantMembershipRepository memberships,
        ITenantRoleResolver roleResolver,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _memberships = memberships;
        _roleResolver = roleResolver;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Guid> CreateAsync(CreateMembershipRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _memberships.GetByUserIdAndTenantIdWithRolesAsync(
            request.UserId,
            request.TenantId,
            cancellationToken);

        if (existing is not null && existing.IsActive)
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.TenantMembership.MembershipAlreadyExists);
        }

        var roles = await _roleResolver.ResolveActiveRolesAsync(
            request.TenantId,
            request.Roles,
            cancellationToken);

        var membership = new TenantMembership(request.TenantId, request.UserId, roles);
        await _memberships.AddAsync(membership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return membership.Id;
    }

    public async Task UpdateRolesAsync(UpdateMembershipRolesRequest request, CancellationToken cancellationToken = default)
    {
        var membership = await _memberships.GetForUpdateWithRolesAsync(request.MembershipId, cancellationToken)
            ?? throw new DomainNotFoundException(DomainErrorMessages.TenantMembership.MembershipNotFound);

        var roles = await _roleResolver.ResolveActiveRolesAsync(
            membership.TenantId,
            request.Roles,
            cancellationToken);

        membership.ReplaceRoles(roles);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAsync(RevokeMembershipRequest request, CancellationToken cancellationToken = default)
    {
        var membership = await _memberships.GetForUpdateWithRolesAsync(request.MembershipId, cancellationToken)
            ?? throw new DomainNotFoundException(DomainErrorMessages.TenantMembership.MembershipNotFound);

        membership.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<MembershipDto>> ListByTenantAsync(
        ListMembershipsByTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var query = _context.TenantMemberships
            .AsNoTracking()
            .Include(x => x.Roles)
            .ThenInclude(x => x.Role)
            .Where(x => x.TenantId == request.TenantId);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.UserId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MembershipDto
            {
                Id = x.Id,
                UserId = x.UserId,
                TenantId = x.TenantId,
                Roles = x.Roles.Select(role => role.Role.Key.Value).ToList(),
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<MembershipDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
