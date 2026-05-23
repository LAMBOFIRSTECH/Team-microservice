using System.Linq.Expressions;
namespace Teams.CORE.Layer.CoreInterfaces;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(object ob, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
    Task<ICollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
    Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
    Task<TEntity?> GenericFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity, CancellationToken cancellationToken = default);
    void Delete(TEntity entity);
    Task DeleteByIdAsync(object ob, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default);
}