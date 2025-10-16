using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Interfaces;

namespace Teams.INFRA.Layer.OtherUOW;

public class TeamStateUnitOfWork : ITeamStateUnitOfWork
{
    public void RecalculateTeamStates(IEnumerable<Team> teams) // pertinence ? 
    {
        foreach (var team in teams)
        {
            if (team.State != TeamState.Archived)
                team.RecalculateStates();
        }
    }
}
