using Kyvo.Application.Services.UserScope;
using Kyvo.Domain.Common;
using Kyvo.Domain.Entities;
using Kyvo.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Kyvo.Infrastructure.Persistence.Interceptors;

public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IUserScope _userScope;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(
        IUserScope userScope,
        IHttpContextAccessor httpContextAccessor)
    {
        _userScope = userScope;
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    private void AddAuditLogs(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var logs = BuildAuditLogs(context.ChangeTracker);
        if (logs.Count > 0)
        {
            context.Set<AuditLog>().AddRange(logs);
        }
    }

    private List<AuditLog> BuildAuditLogs(ChangeTracker changeTracker)
    {
        var logs = new List<AuditLog>();
        var entries = changeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity is not AuditLog)
            .Where(x => x.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            var action = ResolveAction(entry);
            if (action is null)
            {
                continue;
            }

            var tenantId = ResolveTenantId(entry);
            if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            {
                continue;
            }

            logs.Add(new AuditLog(
                tenantId.Value,
                _userScope.UserId == Guid.Empty ? null : _userScope.UserId,
                _userScope.MembershipId,
                action.Value.ToString(),
                entry.Entity.GetType().Name,
                entry.Entity.Id,
                _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString()));
        }

        return logs;
    }

    private static AuditAction? ResolveAction(EntityEntry<BaseEntity> entry)
    {
        return entry.Entity switch
        {
            User when entry.State == EntityState.Added => AuditAction.UserCreated,
            User when entry.State == EntityState.Modified => AuditAction.UserUpdated,
            Tenant when entry.State == EntityState.Added => AuditAction.TenantCreated,
            Tenant when entry.State == EntityState.Modified => AuditAction.TenantUpdated,
            TenantMembership when entry.State == EntityState.Added => AuditAction.MembershipCreated,
            TenantMembership membership when entry.State == EntityState.Modified && !membership.IsActive => AuditAction.MembershipRevoked,
            TenantMembership when entry.State == EntityState.Modified => AuditAction.MembershipRoleUpdated,
            TenantMembershipRole when entry.State == EntityState.Added => AuditAction.MembershipRoleUpdated,
            AuthSession when entry.State == EntityState.Added => AuditAction.SessionCreated,
            AuthSession session when entry.State == EntityState.Modified && session.Status == SessionStatus.Revoked => AuditAction.SessionRevoked,
            TenantInvite when entry.State == EntityState.Added => AuditAction.InviteCreated,
            TenantInvite invite when entry.State == EntityState.Modified && invite.ConsumedAt.HasValue => AuditAction.InviteAccepted,
            _ => null
        };
    }

    private Guid? ResolveTenantId(EntityEntry<BaseEntity> entry)
    {
        return entry.Entity switch
        {
            TenantEntity scoped => scoped.TenantId,
            Tenant tenant => tenant.Id,
            AuthSession session => session.TenantId ?? _userScope.TenantId,
            User => _userScope.TenantId,
            _ => _userScope.TenantId
        };
    }
}
