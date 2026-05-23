using Teams.CORE.Layer.Entities.TeamAggregate;
using Microsoft.EntityFrameworkCore;
namespace Teams.CORE.Layer.CoreInterfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Team> TeamRepository { get; }
    Task CommitAsync(CancellationToken cancellationToken);

    // Les trois ci-dessous sont là uniquement pour le débogage voir l'état de l'entité avant et apres commit
    // ApiContext Context { get; }
    EntityState GetEntityState(object entity);
    // Task<int> CommitAsync(CancellationToken cancellationToken);

}