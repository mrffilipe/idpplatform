using Kyvo.Application.Common;
using Kyvo.Application.Services.AuditLog;
using Kyvo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kyvo.Infrastructure.Services.AuditLog;

public sealed class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;

    public AuditLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AuditLogItemDto>> ListAsync(
        ListAuditLogsRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var query = _context.AuditLogs.AsNoTracking();

        if (request.UserId.HasValue)
        {
            query = query.Where(x => x.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(x => x.Action == request.Action);
        }

        if (!string.IsNullOrWhiteSpace(request.ResourceType))
        {
            query = query.Where(x => x.ResourceType == request.ResourceType);
        }

        if (request.From.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= request.To.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditLogItemDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                UserId = x.UserId,
                MembershipId = x.MembershipId,
                Action = x.Action,
                ResourceType = x.ResourceType,
                ResourceId = x.ResourceId,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogItemDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
