using Teams.CORE.Entities.TeamAG;
using Teams.CORE.CoreInterfaces;
using Teams.INFRA.Dispatchers;

namespace Teams.INFRA.Persistence.DAL;

public class EfUnitOfWork(ApplicationDbContext _context, IDomainEventDispatcher _dispatcher) : IUnitOfWork
{
    private ITeamRepository? _teamRepository;

    // GenericRepository<Team> doit implémenter ITeamRepository pour satisfaire le contrat de l'interface.
    public ITeamRepository TeamRepository
        => _teamRepository ??= (ITeamRepository)new GenericRepository<Team, Guid>(_context);


    // Exposé uniquement à des fins de débogage (inspection directe du DbContext depuis l'extérieur)
    public ApplicationDbContext Context => _context;

    // Uniquement pour déboguer : permet de voir l'état de tracking d'une entité (Added/Modified/Deleted/Unchanged) pendant la transaction
    public string GetEntityStateTracking(object entity) => _context.Entry(entity).State.ToString();

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        // var transaction = await _context.Database.BeginTransactionAsync(cancellationToken); // À activer une fois la chaîne de connexion vers la vraie BD établie
        try
        {
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            // await transaction.CommitAsync(cancellationToken); // À activer une fois la chaîne de connexion vers la vraie BD établie
            await DispatchDomainEventsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // await transaction.RollbackAsync(cancellationToken); // À activer une fois la chaîne de connexion vers la vraie BD établie
            throw;
        }
    }

    /// <summary>
    /// Récupère toutes les entités ayant des DomainEvents en attente, les dispatch, puis les clear
    /// pour éviter un double dispatch si la même entité est modifiée à nouveau avant un prochain commit.
    /// </summary>
    /// <remarks>
    /// Le nom actuel (GetDomainEvents) ne reflète plus exactement le comportement : la méthode ne se contente pas
    /// de récupérer les événements, elle les dispatch et les clear aussi. Un renommage (ex: DispatchDomainEventsAsync)
    /// serait plus explicite, mais implique de mettre à jour tous les appelants.
    /// Note : couplage fort entre l'UnitOfWork et le dispatcher — à surveiller si le pattern Outbox est introduit plus tard (voir remarque en bas de fichier).
    /// </remarks>
    /// <param name="cancellationToken"></param>
    public async Task DispatchDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();
            await _dispatcher.DispatchAsync(events, cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync() => await _context.DisposeAsync().ConfigureAwait(false);

    // Remarque : comme mentionné pour l'UoW, si le dispatching échoue après le Commit, le système peut devenir incohérent
    // (ex: l'équipe existe en base, mais l'événement de création n'a jamais été traité par les autres composants).
    // À terme, envisager le pattern Outbox : stocker les événements dans une table dédiée au moment du commit,
    // puis les faire lire et dispatcher par un processus séparé. Cela garantit qu'aucun événement n'est perdu
    // même si le dispatching échoue, et permet de les rejouer.
}
