using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Teams.INFRA.Layer.Interfaces;

namespace Teams.INFRA.Layer.Persistence.DAL;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class, new()
{
    internal ApiContext context;
    public DbSet<TEntity> dbSet => context.Set<TEntity>();
    public GenericRepository(ApiContext context)
        => this.context = context;

    public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();
    public virtual async Task<TEntity?> GetById(Guid id, params string[] includes)
    {
        IQueryable<TEntity> query = dbSet;
        foreach (var include in includes) query = query.Include(include);
        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
    }
    public virtual IQueryable<TEntity> GetAll(params string[] includes)
    {
        IQueryable<TEntity> query = dbSet;
        foreach (var include in includes) query = query.Include(include);
        return query;
    }
    public virtual IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> expression = null, params string[] includes)
    {
        IQueryable<TEntity> query = dbSet;
        if (expression != null) query = query.Where(expression);
        foreach (var include in includes) query = query.Include(include);
        return query;
    }
    public virtual async Task<TEntity> Create(TEntity entity)
    {
        await dbSet.AddAsync(entity);
        return entity;
    }
    public virtual void Update(TEntity entity)
    {
        dbSet.Attach(entity);
        context.Entry(entity).State = EntityState.Modified;
    }
    public virtual void Delete(TEntity entity)
    {
        if (context.Entry(entity).State == EntityState.Detached)
            dbSet.Attach(entity);
        dbSet.Remove(entity);
    }
    public virtual async Task<bool> IsExist(Expression<Func<TEntity, bool>> expression)
        => await dbSet.AnyAsync(expression);
}
