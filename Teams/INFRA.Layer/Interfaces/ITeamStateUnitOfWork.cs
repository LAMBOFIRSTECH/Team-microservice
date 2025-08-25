using Teams.CORE.Layer.Entities;

namespace Teams.INFRA.Layer.Interfaces;

public interface ITeamStateUnitOfWork
{
    void RecalculateTeamStates(IEnumerable<Team> teams);
}
