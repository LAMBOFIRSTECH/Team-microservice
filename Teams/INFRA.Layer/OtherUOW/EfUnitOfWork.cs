// using Teams.CORE.Layer.Entities;
// using Teams.CORE.Layer.Interfaces;
// using Teams.INFRA.Layer.Dispatchers;

// namespace Teams.INFRA.Layer.UnitOfWork;

// public class EfUnitOfWork(DbContext dbContext, IDomainEventDispatcher dispatcher) : IUnitOfWork
// {
//     private readonly DbContext _dbContext = dbContext;
//     private readonly IDomainEventDispatcher _dispatcher = dispatcher;

//     public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
//     {
//         // Récupère toutes les entités qui ont des DomainEvents
//         var teams = ChangeTracker
//             .Entries<Team>()
//             .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
//             .Select(e => e.Entity)
//             .ToList();
//         unitOfWork.RecalculateTeamStates(teams);
//         var entitiesWithEvents = ChangeTracker
//             .Entries()
//             .Where(e => e.Entity is Team t && t.DomainEvents.Count > 0)
//             .Select(e => (Team)e.Entity)
//             .ToList();


//         // Sauvegarde la transaction
//         var result = await base.SaveChangesAsync(ct);
//         // Dispatch des DomainEvents
//         foreach (var entity in entitiesWithEvents)
//         {
//             var events = entity.DomainEvents.ToList();
//             entity.ClearDomainEvents();
//             await _dispatcher.DispatchAsync(events, ct);
//         }

//         return result;
//     }
// }
