namespace CleanTemplate.Domain.Repositories;

public interface IRepository<TEntity, TKey>
    where TEntity : class
{
    Task<int> CreateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(TKey id);
    Task SaveChanges();
}

