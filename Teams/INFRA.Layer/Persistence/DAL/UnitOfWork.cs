using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Interfaces;
using Teams.INFRA.Layer.Dispatchers;
using Teams.CORE.Layer.CoreInterfaces;
using Microsoft.EntityFrameworkCore;

namespace Teams.INFRA.Layer.Persistence.DAL;

public class UnitOfWork(ApiContext _context, IDomainEventDispatcher _dispatcher) : IUnitOfWork
{
    private GenericRepository<Team>? _teamRepository;

    public IGenericRepository<Team> TeamRepository
        => _teamRepository ??= new GenericRepository<Team>(_context);

    public ApiContext Context => _context; // Uniquement pour déboguer que l'on rajoute le constructeur
    public EntityState GetEntityState(object entity) => _context.Entry(entity).State; // Uniquement pour déboguer afin de voir l'état de l'entité pendant la transaction

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _context.SaveChangesAsync(cancellationToken); // save les modifications dans le context
                                                                //await _context.Database.CommitTransactionAsync(cancellationToken); C'est ici qu'on valide reéllement la transaction en bd
                                                                // await transaction.CommitAsync(cancellationToken);
                                                                // Dispatcher les événements après commit Créer la fonction GetDomainEvents ? coupalge fort du dispatcher avec uow
                                                                
                                                                // Récupère toutes les entités avec des DomainEvents
            var entitiesWithEvents = _context.ChangeTracker
                .Entries<IHasDomainEvents>() // interface que tes entités implémentent
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();
            foreach (var entity in entitiesWithEvents)
            {
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                await _dispatcher.DispatchAsync(events, cancellationToken);
            }
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

    }
    public void Dispose() => _context.Dispose();

}


