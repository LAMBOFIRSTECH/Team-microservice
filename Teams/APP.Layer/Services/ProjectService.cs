using AutoMapper;
using FluentValidation;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;
using Teams.CORE.Layer.ValueObjects;
using Teams.INFRA.Layer.ExternalServices;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.APP.Layer.Services;

public class ProjectService(
    ITeamRepository teamRepository,
    TeamExternalService teamExternalService,
    ILogger<ProjectService> log,
    IValidator<ProjectAssociationDto> projectRecordValidator,
    IMapper mapper
) : IProjectService
{
    public async Task ManageTeamProjectAsync(Guid managerId, string teamName)
    {
        var dto = await teamExternalService.RetrieveProjectAssociationDataAsync();
        if (dto == null)
        {
            LogHelper.Error("❌ Failed to retrieve project association data", log);
            throw new DomainException("Failed to retrieve project association data");
        }
        if (dto.TeamManagerId != managerId || dto.TeamName != teamName)
        {
            LogHelper.Error(
                $"❌ Messages server send: {managerId}, {teamName}. We Received: {dto.TeamManagerId}, {dto.TeamName} from the external service.",
                log
            );
            throw new DomainException(
                $"Mismatched team manager or team name. Expected: {managerId}, {teamName}. Received: {dto.TeamManagerId}, {dto.TeamName}"
            );
        }
        var validationResult = await projectRecordValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            LogHelper.CriticalFailure(log, "Data validation", $"{validationResult}", null);
            throw new DomainException("Project association data are invalid");
        }
        var teamProject = mapper.Map<ProjectAssociation>(dto);
        var existingTeam = await teamRepository.GetTeamByNameAndTeamManagerIdAsync(
            teamProject.TeamName,
            teamProject.TeamManagerId
        );

        if (existingTeam == null)
        {
            LogHelper.Warning(
                $"No team found matching {teamProject.TeamManagerId}, {teamProject.TeamName}",
                log
            );
            throw new DomainException(
                $"No team found matching {teamProject.TeamManagerId}, {teamProject.TeamName}"
            );
        }
        if (existingTeam.State == TeamState.Complete)
        {
            foreach (var item in teamProject.Details)
            {
                LogHelper.Warning(
                    $"Team {teamProject.TeamName} already has an active project name's [ {item.ProjectName} ].",
                    log
                );
                throw new DomainException(
                    $"Team {teamProject.TeamName} already has an active project."
                );
            }
        }
        var projectState = teamProject.Details.Any(p => p.State == ProjectState.Active);
        if (!projectState)
            await RemoveTeamProjectAsync(existingTeam, teamProject);
        await AddTeamProjectAsync(existingTeam, teamProject);
    }

    private async Task AddTeamProjectAsync(Team existingTeam, ProjectAssociation project)
    {
        existingTeam.AttachProjectToTeam(project, true);
        await teamRepository.UpdateTeamAsync(existingTeam);
        LogHelper.Info(
            $"🔗 📁 👥 Team {project.TeamName} has been attached to [{project.Details.Count}] project(s) successfully. ",
            log
        );
    }

    private async Task RemoveTeamProjectAsync(Team existingTeam, ProjectAssociation project)
    {
        existingTeam.RemoveProjectsIfExpiredOrSuspended(false);
        await teamRepository.UpdateTeamAsync(existingTeam);
        LogHelper.Info(
            $"✅ Team 🧑‍🤝‍🧑 {project.TeamName} successfully removed from 🗂️ {project.Details.Count} project(s).",
            log
        );
    }
}
