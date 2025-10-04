using Teams.CORE.Layer.Entities.TeamAggregate;

namespace Teams.INFRA.Layer.Interfaces;

public interface ITeamStateUnitOfWork
{
    void RecalculateTeamStates(IEnumerable<Team> teams);
}
