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
    public async Task<ProjectAssociation> GetProjectAssociationDataAsync(Guid? managerId, string teamName)
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
        return mapper.Map<ProjectAssociation>(dto);

    }
    public async Task ManageTeamProjectAsync(Guid operationId, string operationName)
    {
        var teamProject = await GetProjectAssociationDataAsync(operationId, operationName);
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
        if (teamProject.HasSuspendedProject())
            throw new DomainException("At least one project must be active.");

        await AddProjectToTeamAsync(existingTeam, teamProject);
    }
    private async Task AddProjectToTeamAsync(Team existingTeam, ProjectAssociation teamProject)
    {
        if (teamProject == null || teamProject.IsEmpty())
            throw new DomainException("Project association data cannot be null");

        if (!teamProject.HasActiveProject())
            throw new DomainException("Project must be active to be associated with a team.");

        if (teamProject.Details.Count > 3)
            throw new DomainException("A team cannot be associated with more than 3 projects.");

        existingTeam.AssignProject(teamProject);
        await teamRepository.UpdateTeamAsync(existingTeam);
        LogHelper.Info(
            $"🔗 📁 👥 Team {teamProject.TeamName} has been attached to [{teamProject.Details.Count}] project(s) successfully.",
            log
        );
    }
    public async Task SuspendedProjectAsync(Guid managerId, string projectName)
    {
        var existingTeams = await teamRepository.GetTeamsByManagerIdAsync(managerId);
        var suspendedTeam = existingTeams
             .FirstOrDefault(t => t.Project.Details.Any(d => d.ProjectName == projectName));
        if (suspendedTeam == null)
        {
            LogHelper.Warning(
                $"No team found matching {managerId}, {projectName} with a suspended project",
                log
            );
            throw new DomainException(
                $"No team found matching {managerId}, {projectName} with a suspended project"
            );
        }
        suspendedTeam.RemoveSuspendedProjects(projectName);
        await teamRepository.UpdateTeamAsync(suspendedTeam);
        LogHelper.Info(
            $"✅ The suspended project 🗂️ [{projectName}] successfully removed from Team 🧑‍🤝‍🧑 [[{suspendedTeam.Name.Value}]].",
            log
        );
    }
}
