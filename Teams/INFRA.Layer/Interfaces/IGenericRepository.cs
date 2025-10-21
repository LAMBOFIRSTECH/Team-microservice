using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Teams.INFRA.Layer.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : class, new()
{
    public DbSet<TEntity> dbSet { get; }
    public Task<TEntity?> GetById(CancellationToken cancellationToken, Guid id, params string[] includes);
    public IQueryable<TEntity> GetAll(params string[] includes);
    public IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> expression = null, params string[] includes);
    public Task<TEntity> Create(TEntity entity, CancellationToken cancellationToken);
    public void Update(TEntity entity);
    public void Delete(TEntity entity);
    public Task<bool> IsExist(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken);
}