using Teams.CORE.Layer.Entities;

namespace Teams.APP.Layer.Interfaces;

public interface ITeamStateUnitOfWork
{
    void RecalculateTeamStates(IEnumerable<Team> teams);
}
