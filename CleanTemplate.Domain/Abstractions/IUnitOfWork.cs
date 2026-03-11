namespace CleanTemplate.Domain.Abstractions;

public interface IUnitOfWork : IDisposable
{
    Task BeginTransaction(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

