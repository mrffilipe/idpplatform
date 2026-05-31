using Kyvo.Application.Common;
using Kyvo.Application.Services.TenantRoles;
using Kyvo.Application.Services.UnitOfWork;
using Kyvo.Domain.Exceptions;
using Kyvo.Domain.Repositories;
using Kyvo.Domain.ValueObjects;

namespace Kyvo.Infrastructure.Services.TenantRoles;

public sealed class TenantRoleService : ITenantRoleService
{
    private readonly ITenantRepository _tenants;
    private readonly ITenantRoleRepository _roles;
    private readonly IUnitOfWork _unitOfWork;

    public TenantRoleService(
        ITenantRepository tenants,
        ITenantRoleRepository roles,
        IUnitOfWork unitOfWork)
    {
        _tenants = tenants;
        _roles = roles;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(CreateTenantRoleRequest request, CancellationToken cancellationToken = default)
    {
        _ = await _tenants.GetForUpdateAsync(request.TenantId, cancellationToken)
            ?? throw new DomainNotFoundException(DomainErrorMessages.Tenant.TenantNotFound);

        var key = new TenantRoleKey(request.Key);
        if (await _roles.KeyAlreadyExistsAsync(request.TenantId, key, cancellationToken))
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.TenantRole.RoleAlreadyExists);
        }

        var role = new Domain.Entities.TenantRole(request.TenantId, key, request.Name, request.Description);
        await _roles.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return role.Id;
    }

    public async Task UpdateAsync(UpdateTenantRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _roles.GetForUpdateAsync(request.RoleId, cancellationToken)
            ?? throw new DomainNotFoundException(DomainErrorMessages.TenantRole.RoleNotFound);

        role.UpdateDetails(request.Name, request.Description);
        if (request.IsActive)
        {
            role.Activate();
        }
        else
        {
            role.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<TenantRoleDto>> ListAsync(
        ListTenantRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var roles = await _roles.ListByTenantIdAsync(
            request.TenantId,
            request.IncludeInactive,
            cancellationToken);

        var items = roles
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new TenantRoleDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                Key = x.Key.Value,
                Name = x.Name,
                Description = x.Description,
                IsSystem = x.IsSystem,
                IsActive = x.IsActive
            })
            .ToList();

        return new PagedResult<TenantRoleDto>
        {
            Items = items,
            Total = roles.Count,
            Page = page,
            PageSize = pageSize
        };
    }
}
