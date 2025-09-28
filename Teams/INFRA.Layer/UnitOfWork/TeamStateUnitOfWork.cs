using Teams.CORE.Layer.Entities;
using Teams.INFRA.Layer.Interfaces;

namespace Teams.INFRA.Layer.UnitOfWork;

public class TeamStateUnitOfWork : ITeamStateUnitOfWork
{
    public void RecalculateTeamStates(IEnumerable<Team> teams)
    {
        foreach (var team in teams)
        {
            if (team.State != TeamState.Archived)
                team.RecalculateStates();
        }
    }
}
