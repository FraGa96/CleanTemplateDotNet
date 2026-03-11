using CleanTemplate.Domain.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace CleanTemplate.Infrastructure.Persistence;

internal class UnitOfWork(AppDbContext dbContext, ILogger<UnitOfWork> logger) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public async Task BeginTransaction(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Transaction started");
        _transaction ??= await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction is not null)
            {
                logger.LogInformation("Transaction committed");
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction is not null)
            {
                logger.LogInformation("Transaction rolled back");
                await _transaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public void Dispose() => _transaction?.Dispose();

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            logger.LogInformation("Transaction disposed");
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}

