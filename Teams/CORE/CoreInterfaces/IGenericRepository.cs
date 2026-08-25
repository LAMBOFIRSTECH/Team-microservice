using System.Linq.Expressions;

namespace Teams.CORE.CoreInterfaces;

public interface IGenericRepository<TEntity, TKey> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default, IEnumerable<Expression<Func<TEntity, object>>>? includes = null);

    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default, IEnumerable<Expression<Func<TEntity, object>>>? includes = null);

    Task<IReadOnlyList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, IEnumerable<Expression<Func<TEntity, object>>>? includes = null);

    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, IEnumerable<Expression<Func<TEntity, object>>>? includes = null);

    TEntity Add(TEntity entity);

    void Update(TEntity entity);

    void Delete(TEntity entity);

    Task<bool> DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}