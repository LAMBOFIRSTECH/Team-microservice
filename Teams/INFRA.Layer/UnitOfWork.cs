using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Persistence.DAL;
using Teams.INFRA.Layer.Interfaces;
using Teams.INFRA.Layer.Dispatchers;
using Teams.CORE.Layer.CoreInterfaces;

namespace Teams.INFRA.Layer;

public class UnitOfWork(ApiContext _context,  IDomainEventDispatcher _dispatcher) : IUnitOfWork
{
    private GenericRepository<Team>? _teamRepository;

    public IGenericRepository<Team> TeamRepository
        =>  _teamRepository ??= new GenericRepository<Team>(_context);

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        // Récupère toutes les entités avec des DomainEvents
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<IHasDomainEvents>() // interface que tes entités implémentent
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();
        await _context.SaveChangesAsync();
        // Dispatcher les événements après commit
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();
            await _dispatcher.DispatchAsync(events, cancellationToken);
        }
    }
    public void Dispose() => _context.Dispose();

}