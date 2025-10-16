using AutoMapper;
using FluentValidation;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.CoreServices;
using Teams.CORE.Layer.Entities.GeneralValueObjects;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.ExternalServices;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.APP.Layer.Services;

public class ProjectService(
    ITeamRepository teamRepository,
    TeamExternalService teamExternalService,
    ProjectLifeCycleCoreService projectLifeCycleCore,
    TeamLifeCycleCoreService teamLifeCycleCoreService,
    ILogger<ProjectService> log,
    IValidator<ProjectAssociationDto> projectRecordValidator,
    IMapper mapper
) : IProjectService
{
    public async Task<ProjectAssociation> GetProjectAssociationDataAsync(Guid? managerId, string teamName)
    {
        var dto = await teamExternalService.RetrieveProjectAssociationDataAsync();
        if (dto == null)
        {
            LogHelper.Error("❌ Failed to retrieve project association data", log);
            throw new InvalidOperationException("Failed to retrieve project association data");
        }

        if (dto.TeamManagerId != managerId || dto.TeamName != teamName)
        {
            LogHelper.Error(
                $"❌ Mismatch: Expected [{managerId}, {teamName}], Received [{dto.TeamManagerId}, {dto.TeamName}]",
                log
            );
            throw new InvalidOperationException("Mismatched team manager or team name");
        }

        var validationResult = await projectRecordValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            LogHelper.CriticalFailure(log, "Data validation", $"{validationResult}", null);
            throw new InvalidOperationException("Project association data is invalid");
        }

        return mapper.Map<ProjectAssociation>(dto);
    }

    public async Task ManageTeamProjectAsync(Guid managerId, string teamName)
    {
        var teamProject = await GetProjectAssociationDataAsync(managerId, teamName);
        var existingTeam = await teamRepository.GetTeamByNameAndTeamManagerIdAsync(teamProject.TeamName, teamProject.TeamManagerId);
        if (existingTeam == null)
        {
            LogHelper.Warning($"No team found for [{teamProject.TeamManagerId}, {teamProject.TeamName}]", log);
            throw new InvalidOperationException("No matching team found");
        }

        if (!teamProject.HasActiveProject())
            throw new InvalidOperationException("At least one project must be active to associate with the team");

        projectLifeCycleCore.AddProjectToTeamAsync(existingTeam, teamProject);
        await teamRepository.UpdateTeamAsync(existingTeam);
        LogHelper.Info(
           $"🔗 Team '{teamProject.TeamName}' successfully attached to [{teamProject.Details.Count}] project(s)",
           log
       );
    }

    public async Task SuspendProjectAsync(Guid managerId, string projectName)
    {
        var existingTeams = await teamRepository.GetTeamsByManagerIdAsync(managerId);
        var suspendedTeam = await projectLifeCycleCore.SuspendProjectAsync(managerId, projectName, existingTeams);
        await teamRepository.UpdateTeamAsync(suspendedTeam);
        LogHelper.Info(
            $"✅ Project '{projectName}' successfully removed from Team '{suspendedTeam.Name.Value}'",
            log
        );
    }
    public TeamDetailsDto BuildDto(Team team)
    {
        var teamDto = mapper.Map<TeamDetailsDto>(team);
        var projectAssociation = team.Project;
        if (projectAssociation == null || projectAssociation.Details.Count == 0)
        {
            teamDto.HasAnyProject = false;
            teamDto.ProjectNames = null;
        }
        else
        {
            teamDto.TeamExpirationDate = projectAssociation
                .GetprojectMaxEndDate()
                .ToString("dd-MM-yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

            teamDto.HasAnyProject = true;
            teamDto.TeamManagerId = projectAssociation.TeamManagerId;
            teamDto.Name = projectAssociation.TeamName;
            teamDto.ProjectNames = projectAssociation.Details.Select(d => d.ProjectName).ToList();
        }
        teamDto.State = teamLifeCycleCoreService.MatureTeam(team);
        return teamDto;
    }

}
