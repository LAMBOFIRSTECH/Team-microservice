using Teams.CORE.Layer.Entities.TeamAggregate;
using Microsoft.IdentityModel.Logging;
using Teams.APP.Layer.Interfaces;

namespace Teams.APP.Layer.Services;
public class TeamProjectLifeCycle(ITeamRepository teamRepository, ILogger<TeamProjectLifeCycle> _log) : ITeamProjectLifeCycle
{
    public async Task RemoveProjects(CancellationToken ct)
    {
        var teams = await teamRepository.GetTeamsWithExpiredProject(ct);
        foreach (var team in teams)
        {
            team.RemoveExpiredProjects();
            await teamRepository.UpdateTeamAsync(team, ct);
            LogHelper.LogInformation($"✅ Project has been dissociated from team {team.Name}", _log);
        }
    }
}