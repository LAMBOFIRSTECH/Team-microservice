using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Persistence.DAL;

namespace Teams.INFRA.Layer.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Team> TeamRepository { get; }
    Task SaveAsync(CancellationToken cancellationToken);
    ApiContext Context { get; }
    EntityState GetEntityState(object entity);

}