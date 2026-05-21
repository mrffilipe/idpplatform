using IdPPlatform.Application.Common;
using IdPPlatform.Application.Exceptions;
using IdPPlatform.Application.Services.AppService;
using IdPPlatform.Application.Services.TenantResolutionCache;
using IdPPlatform.Application.Services.UnitOfWork;
using IdPPlatform.Domain.Constants;
using IdPPlatform.Domain.Entities;
using IdPPlatform.Domain.Exceptions;
using IdPPlatform.Domain.Repositories;
using IdPPlatform.Domain.ValueObjects;
using IdPPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdPPlatform.Infrastructure.Services.AppService;

public sealed class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _applications;
    private readonly IApplicationClientRepository _clients;
    private readonly IApplicationTenantRepository _applicationTenants;
    private readonly ITenantRepository _tenants;
    private readonly ITenantRoleRepository _roles;
    private readonly ITenantMembershipRepository _memberships;
    private readonly IUserRepository _users;
    private readonly ITenantResolutionCache _tenantResolutionCache;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public ApplicationService(
        IApplicationRepository applications,
        IApplicationClientRepository clients,
        IApplicationTenantRepository applicationTenants,
        ITenantRepository tenants,
        ITenantRoleRepository roles,
        ITenantMembershipRepository memberships,
        IUserRepository users,
        ITenantResolutionCache tenantResolutionCache,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _applications = applications;
        _clients = clients;
        _applicationTenants = applicationTenants;
        _tenants = tenants;
        _roles = roles;
        _memberships = memberships;
        _users = users;
        _tenantResolutionCache = tenantResolutionCache;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Guid> CreateAsync(CreateApplicationRequest request, CancellationToken cancellationToken = default)
    {
        var application = new Domain.Entities.Application(request.Name, request.Slug, request.Type);
        await _applications.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return application.Id;
    }

    public async Task<Guid> CreateClientAsync(
        CreateApplicationClientRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.ActorPlatformRoles.Any(role => PlatformRoleDefaults.AdministrativeKeys.Contains(role)))
        {
            throw new ForbiddenApplicationException(ApplicationErrorMessages.Auth.UserHasNoTenantAccess);
        }

        var client = new ApplicationClient(
            request.ApplicationId,
            request.ClientId,
            request.ClientSecretHash,
            request.ClientType,
            ApplicationClientListFields.ToRedirectUrisJson(request.RedirectUris),
            ApplicationClientListFields.ToAllowedScopesJson(request.AllowedScopes),
            request.AccessTokenTtlSeconds);

        await _clients.AddAsync(client, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return client.Id;
    }

    public async Task<ProvisionApplicationTenantResult> ProvisionTenantAsync(
        ProvisionApplicationTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.ActorPlatformRoles.Any(role => PlatformRoleDefaults.AdministrativeKeys.Contains(role)))
        {
            throw new ForbiddenApplicationException(ApplicationErrorMessages.Auth.UserHasNoTenantAccess);
        }

        var application = await _applications.GetByIdAsync(request.ApplicationId, cancellationToken)
            ?? throw new DomainNotFoundException(ApplicationErrorMessages.Application.NotFound);

        var tenantKey = new TenantKey(request.TenantKey);
        if (await _tenants.KeyAlreadyExistsAsync(tenantKey, cancellationToken))
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

        var tenant = new Domain.Entities.Tenant(request.TenantName, tenantKey);
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

        var membership = new TenantMembership(tenant.Id, initialAdministratorUserId, [ownerRole]);
        await _memberships.AddAsync(membership, cancellationToken);

        var applicationTenant = new ApplicationTenant(
            application.Id,
            tenant.Id,
            request.ExternalCustomerId,
            request.PlanCode);

        if (await _applicationTenants.ExistsAsync(application.Id, tenant.Id, cancellationToken))
        {
            throw new DomainBusinessRuleException(DomainErrorMessages.ApplicationTenant.MappingAlreadyExists);
        }

        await _applicationTenants.AddAsync(applicationTenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _tenantResolutionCache.InvalidateByIdentifierAsync(tenantKey.Value, cancellationToken);

        return new ProvisionApplicationTenantResult
        {
            ApplicationId = application.Id,
            TenantId = tenant.Id,
            MembershipId = membership.Id
        };
    }

    public async Task<ApplicationDto?> GetByIdAsync(
        GetApplicationByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .AsNoTracking()
            .Where(x => x.Id == request.ApplicationId)
            .Select(x => new ApplicationDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Type = x.Type,
                IsSystem = x.IsSystem
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<ApplicationDto>> ListAsync(
        ListApplicationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var query = _context.Applications.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ApplicationDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Type = x.Type,
                IsSystem = x.IsSystem
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ApplicationDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
