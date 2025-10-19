using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Teams.INFRA.Layer.Interfaces;
public interface IGenericRepository<TEntity> where TEntity : class, new()
{
    public DbSet<TEntity> dbSet { get; }
    public Task<TEntity?> GetById(Guid id, params string[] includes);
    public IQueryable<TEntity> GetAll(params string[] includes);
    public IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> expression = null, params string[] includes);
    public Task<TEntity> Create(TEntity entity);
    public void Update(TEntity entity);
    public void Delete(TEntity entity);
    public Task<int> SaveChangesAsync();
    public Task<bool> IsExist(Expression<Func<TEntity, bool>> expression);
}