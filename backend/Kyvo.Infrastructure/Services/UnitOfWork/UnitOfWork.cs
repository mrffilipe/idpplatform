using Kyvo.Application.Services.UnitOfWork;
using Kyvo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kyvo.Infrastructure.Services.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteInSerializableTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(
            cancellationToken);
        await _context.Database.ExecuteSqlRawAsync("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE", cancellationToken);

        await operation(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
