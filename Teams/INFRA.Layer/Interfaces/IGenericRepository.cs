using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Teams.INFRA.Layer.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetById(CancellationToken cancellationToken, Guid id, params string[] includes);
    IQueryable<TEntity> GetAll(CancellationToken cancellationToken = default, params string[] includes);
    IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> expression = null, params string[] includes);
    Task<TEntity> Create(TEntity entity, CancellationToken cancellationToken);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task<bool> IsExist(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken);
}