using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Teams.CORE.CoreInterfaces;

namespace Teams.INFRA.Persistence.DAL;

public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey> where TEntity : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default, IEnumerable<Expression<Func<TEntity, object>>>? includes = null)
    {
        // FindAsync est privilégié : il vérifie d'abord le ChangeTracker en mémoire avant d'aller en base.
        // Si des includes sont nécessaires, on retombe sur une requête explicite car FindAsync ne les supporte pas.
        if (includes is null)
            return await _dbSet.FindAsync(new object?[] { id }, cancellationToken).ConfigureAwait(false);

        IQueryable<TEntity> query = _dbSet.AsNoTracking();
        foreach (var include in includes)
            query = query.Include(include);

        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Récupère une entité par son identifiant en utilisant la clé primaire définie dans le DbContext, peu importe son nom.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public virtual async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        // Utilise la clé primaire définie dans le DbContext (peu importe son nom)
        var keyName = _context.Model
                            .FindEntityType(typeof(TEntity))?
                            .FindPrimaryKey()?
                            .Properties
                            .Select(x => x.Name)
                            .SingleOrDefault();
        if (keyName == null)
            throw new InvalidOperationException(string.Format("No primary key found for {0}", typeof(TEntity).Name));

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<object>(e, keyName).Equals(id), cancellationToken)
            .ConfigureAwait(false);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default, IEnumerable<Expression<Func<TEntity, object>>>? includes = null)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        if (includes is not null)
            foreach (var include in includes)
                query = query.Include(include);

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<IReadOnlyList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, IEnumerable<Expression<Func<TEntity, object>>>? includes = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        IQueryable<TEntity> query = _dbSet.AsNoTracking().Where(predicate);

        if (includes is not null)
            foreach (var include in includes)
                query = query.Include(include);

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, IEnumerable<Expression<Func<TEntity, object>>>? includes = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        IQueryable<TEntity> query = _dbSet.AsNoTracking().Where(predicate);

        if (includes is not null)
            foreach (var include in includes)
                query = query.Include(include);

        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    // Add est volontairement synchrone : pour la quasi-totalité des providers relationnels (SQL Server IDENTITY,
    // PostgreSQL SERIAL, InMemory), l'ajout ne nécessite aucun aller-retour DB. AddAsync n'a d'intérêt réel
    // que pour certains générateurs de clé (ex: HiLo) ou Cosmos DB.
    public virtual TEntity Add(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Add(entity);
        return entity;
    }

    public virtual void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public virtual void Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (_context.Entry(entity).State == EntityState.Detached)
            _dbSet.Attach(entity);
        _dbSet.Remove(entity);
    }

    public virtual async Task<bool> DeleteByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(new object?[] { id }, cancellationToken).ConfigureAwait(false);
        if (entity is null) return false;

        _dbSet.Remove(entity);
        return true;
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return await _dbSet.AnyAsync(predicate, cancellationToken).ConfigureAwait(false);
    }
}