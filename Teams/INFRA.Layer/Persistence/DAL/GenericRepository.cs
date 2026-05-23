using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.CoreInterfaces;

namespace Teams.INFRA.Layer.Persistence.DAL;
public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    protected readonly ApiContext context;
    protected readonly DbSet<TEntity> dbSet;

    public GenericRepository(ApiContext context)
    {
        this.context = context;
        this.dbSet = context.Set<TEntity>();
    }
    public virtual async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = dbSet.AsNoTracking();
        foreach (var include in includes)
            query = query.Include(include);
        return await query.FirstOrDefaultAsync(e => EF.Property<object>(e, "Id") == id,cancellationToken);
    }

    public virtual async Task<ICollection<TEntity>> GetAllAsync(CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = dbSet.AsNoTracking();

        foreach (var include in includes)
            query = query.Include(include);

        return await query.ToListAsync(cancellationToken);
    }
    public virtual async Task<List<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = dbSet.AsNoTracking();

        if (predicate != null)
            query = query.Where(predicate);

        foreach (var include in includes)
            query = query.Include(include);

        return await query.ToListAsync(cancellationToken);
    }
    public virtual async Task<TEntity?> GenericFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,CancellationToken cancellationToken,params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = dbSet.AsNoTracking();
        if (predicate != null)
            query = query.Where(predicate);

        foreach (var include in includes)
            query = query.Include(include);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual void Update(TEntity entity, CancellationToken cancellationToken)
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
   public virtual async Task DeleteByIdAsync(object ob, CancellationToken cancellationToken)
   {
        var entity = await dbSet.FindAsync(new[] { ob }, cancellationToken);
        if (entity != null) dbSet.Remove(entity);
        
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken)
        => await dbSet.AnyAsync(expression, cancellationToken); // à revoir son importance dans le process
}