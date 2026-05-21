using IdPPlatform.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using AppEntity = IdPPlatform.Domain.Entities.Application;

namespace IdPPlatform.Infrastructure.Persistence.Repositories;

public sealed class ApplicationRepository : IApplicationRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(AppEntity application, CancellationToken cancellationToken = default)
    {
        return _context.Applications
            .AddAsync(application, cancellationToken)
            .AsTask();
    }

    public Task<AppEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Applications
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> SlugAlreadyExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalized = slug.Trim().ToLowerInvariant();
        return _context.Applications
            .AnyAsync(x => x.Slug == normalized, cancellationToken);
    }
}
