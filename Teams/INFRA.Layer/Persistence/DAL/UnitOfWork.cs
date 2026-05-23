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
        // var transaction = await _context.Database.BeginTransactionAsync(cancellationToken); Une fois que la chaine de connexion vers la bd réelle sera établit 
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            //await transaction.CommitAsync(cancellationToken); Une fois que la chaine de connexion vers la bd réelle sera établit          
            // Récupère toutes les entités avec des DomainEvents
            await GetDomainEvents(cancellationToken);
        }
        catch (Exception)
        {
            // await transaction.RollbackAsync(cancellationToken); Une fois que la chaine de connexion vers la bd réelle sera établit 
            throw;
        }

    }
    /// <summary>
    /// Récupère toutes les entités qui ont des DomainEvents, les dispatch, puis les clear pour éviter de les redispacher si jamais la même entité est modifiée à nouveau et que le commit est rappelé
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task GetDomainEvents(CancellationToken cancellationToken = default)
    {
        // Dispatcher les événements après commit Créer la fonction GetDomainEvents ? coupalge fort du dispatcher avec uow
        var entitiesWithEvents = _context.ChangeTracker
                                                        .Entries<IHasDomainEvents>()
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
    public void Dispose() => _context.Dispose();


    // rmq: Comme mentionné pour l'UoW, si ton dispatching échoue après le Commit, ton système peut devenir incohérent (l'équipe existe, mais le projet n'est pas au courant). À terme, regarde le pattern Outbox
    // outbox: stocker les événements dans une table dédiée de ta base de données lors du commit, puis avoir un processus séparé qui lit ces événements et les dispatch. Cela garantit que même sile dispatching échoue, tu n'as pas de perte d'événements et tu peux les reprocesser.
}
