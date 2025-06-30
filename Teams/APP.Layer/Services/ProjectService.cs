#nullable enable
using Teams.CORE.Layer.BusinessExceptions;
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
    public async Task ManageTeamteamProjectAsync()
    {
        var dto = await teamExternalService.RetrieveProjectAssociationDataAsync();

        var teamProject = new ProjectAssociation(
            dto.TeamManagerIdDto,
            dto.TeamNameDto,
            dto.ProjectStartDateDto
        );

        var existingTeam = await teamRepository.GetTeamByNameAndTeamManagerIdAsync(
            teamProject.TeamManagerId,
            teamProject.TeamName
        );

        if (existingTeam == null)
            throw new DomainException(
                $"No team found matching {teamProject.TeamManagerId}, {teamProject.TeamName}"
            );

        existingTeam.AttachProjectToTeam(teamProject, true);
        await teamRepository.UpdateTeamAsync(existingTeam);
    }
}
