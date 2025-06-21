using Teams.APP.Layer.Services;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers.Events;

public class TeamEvent(
    ITeamRepository teamRepository,
    EmployeeService employeeService,
    ILogger<TeamEvent> logger
)
{
    public async Task SafeAddTeamMemberAsync()
    {
        var allTeams = await teamRepository.GetAllTeamsAsync();
        if (allTeams is null || !allTeams.Any())
        {
            logger.LogInformation("There is no Team in db. Job not executed.");
            return;
        }
        await employeeService.AddTeamMemberAsync();
    }
}
