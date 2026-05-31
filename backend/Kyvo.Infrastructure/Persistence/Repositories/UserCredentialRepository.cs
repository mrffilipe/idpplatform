using Kyvo.Domain.Entities;
using Kyvo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kyvo.Infrastructure.Persistence.Repositories;

public sealed class UserCredentialRepository : IUserCredentialRepository
{
    private readonly ApplicationDbContext _context;

    public UserCredentialRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(UserCredential credential, CancellationToken cancellationToken = default)
    {
        return _context.UserCredentials
            .AddAsync(credential, cancellationToken)
            .AsTask();
    }

    public Task<UserCredential?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _context.UserCredentials
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }
}
