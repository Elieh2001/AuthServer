using System.Linq.Expressions;

namespace AuthServer.Domain.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    // Synchronous methods
    TEntity GetById(Guid id);
    IEnumerable<TEntity> GetAll();
    IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
    TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

    void Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);

    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);

    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);

    // Asynchronous methods
    Task<TEntity> GetByIdAsync(Guid id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);

    // Queryable for complex queries
    IQueryable<TEntity> Query();
    IQueryable<TEntity> QueryNoTracking();

    // Count
    int Count(Expression<Func<TEntity, bool>> predicate = null);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null);

    // Exists
    bool Exists(Expression<Func<TEntity, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
}