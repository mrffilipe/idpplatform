namespace Kyvo.Application.Services.UnitOfWork;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task ExecuteInSerializableTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
}
