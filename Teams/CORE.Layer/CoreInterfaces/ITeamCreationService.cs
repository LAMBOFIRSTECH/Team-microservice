using Teams.CORE.Layer.Entities.TeamAggregate;

namespace Teams.CORE.Layer.CoreInterfaces;
public interface ITeamCreationService
{
    Task<Team> CreateUniqueTeamAsync(string name, Guid managerId, IEnumerable<Guid> members, CancellationToken cancellationToken= default);
}