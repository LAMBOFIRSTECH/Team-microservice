using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.Entities;

namespace Teams.APP.Layer.UnitOfWork;

public class TeamStateUnitOfWork : ITeamStateUnitOfWork
{
    public void RecalculateTeamStates(IEnumerable<Team> teams)
    {
        foreach (var team in teams)
        {
            team.RecalculateState();
        }
    }
}
