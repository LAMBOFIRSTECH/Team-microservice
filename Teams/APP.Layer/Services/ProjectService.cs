using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;
using Teams.CORE.Layer.ValueObjects;
using Teams.INFRA.Layer.ExternalServices;

namespace Teams.APP.Layer.Services;

public class ProjectService(
    ITeamRepository teamRepository,
    ILogger<ProjectService> log,
    TeamExternalService teamExternalService
)
{
    public async Task ManageTeamProjectAssociationAsync()
    {
        var dto = await teamExternalService.RetrieveProjectAssociationDataAsync();

        var projectAssociation = new ProjectAssociation(
            dto.TeamManagerIdDto,
            dto.TeamNameDto,
            dto.ProjectStartDateDto
        );
        var existingTeams = await teamRepository.GetAllTeamsAsync();
        var dbTeam = await teamRepository.GetTeamByNameAndTeamManagerIdAsync(
            projectAssociation.TeamManagerId,
            projectAssociation.TeamName
        );
        var memberIds = dbTeam?.MemberIds.Select(m => m).ToList() ?? new List<Guid>();
        var team = Team.Create(
            projectAssociation.TeamName,
            projectAssociation.TeamManagerId,
            memberIds,
            existingTeams,
            true,
            projectAssociation
        );
        await teamRepository.CreateTeamAsync(team);
    }
}
