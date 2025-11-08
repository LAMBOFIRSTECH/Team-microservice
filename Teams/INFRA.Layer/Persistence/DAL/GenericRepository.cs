using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Teams.INFRA.Layer.Interfaces;

namespace Teams.INFRA.Layer.Persistence.DAL;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    internal ApiContext context;
    public DbSet<TEntity> dbSet => context.Set<TEntity>();
    public GenericRepository(ApiContext context)
        => this.context = context;

    public virtual async Task<TEntity?> GetById(CancellationToken cancellationToken, Guid id, params string[] includes)
    {
        IQueryable<TEntity> query = dbSet;
        foreach (var include in includes) query = query.Include(include);
        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
    }
    public virtual IQueryable<TEntity> GetAll(CancellationToken cancellationToken = default, params string[] includes)
    {
        IQueryable<TEntity> query = dbSet.AsQueryable();
        foreach (var include in includes) query = query.Include(include);
        return query.ToListAsync(cancellationToken).Result.AsQueryable();
    }
    
    public virtual IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> expression = null!, params string[] includes)
    {
        IQueryable<TEntity> query = dbSet;
        if (expression != null) query = query.Where(expression);
        foreach (var include in includes) query = query.Include(include);
        return query;
    }
    public virtual async Task<TEntity> Create(TEntity entity, CancellationToken cancellationToken)
    {
        await dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }
    public virtual void Update(TEntity entity)
    {
        dbSet.Attach(entity);
        context.Entry(entity).State = EntityState.Modified;
    }
    public virtual void Delete(TEntity entity )
    {
        if (context.Entry(entity).State == EntityState.Detached)
            dbSet.Attach(entity);
        dbSet.Remove(entity);
    }
    public virtual async Task<bool> IsExist(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken)
        => await dbSet.AnyAsync(expression, cancellationToken);
}
