using Kyvo.Domain.Entities;
using Kyvo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kyvo.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .AddAsync(user, cancellationToken)
            .AsTask();
    }

    public Task<User?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Users
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _context.Users
            .FirstOrDefaultAsync(x => x.Email.Value == normalized, cancellationToken);
    }

    public Task<bool> EmailAlreadyExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _context.Users
            .AnyAsync(x => x.Email.Value == normalized, cancellationToken);
    }
}
