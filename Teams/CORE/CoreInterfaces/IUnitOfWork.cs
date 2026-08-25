using Teams.CORE.Entities.TeamAG;
namespace Teams.CORE.CoreInterfaces;
public interface IUnitOfWork : IAsyncDisposable
{
    ITeamRepository TeamRepository { get; }
    Task CommitAsync(CancellationToken cancellationToken);
    string GetEntityStateTracking(object entity);
}