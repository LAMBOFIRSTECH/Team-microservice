using Teams.CORE.Layer.Entities.TeamAggregate;

namespace Teams.INFRA.Layer.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Team> TeamRepository { get; }
    Task SaveAsync(CancellationToken cancellationToken);

}