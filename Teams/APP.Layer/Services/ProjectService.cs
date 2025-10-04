using AutoMapper;
using FluentValidation;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Entities.GeneralValueObjects;
using Teams.CORE.Layer.Entities.TeamAggregate;
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
                $"❌ Mismatch: Expected [{managerId}, {teamName}], Received [{dto.TeamManagerId}, {dto.TeamName}]",
                log
            );
            throw new DomainException("Mismatched team manager or team name");
        }

        var validationResult = await projectRecordValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            LogHelper.CriticalFailure(log, "Data validation", $"{validationResult}", null);
            throw new DomainException("Project association data is invalid");
        }

        return mapper.Map<ProjectAssociation>(dto);
    }

    public async Task ManageTeamProjectAsync(Guid managerId, string teamName)
    {
        var teamProject = await GetProjectAssociationDataAsync(managerId, teamName);

        var existingTeam = await teamRepository.GetTeamByNameAndTeamManagerIdAsync(
            teamProject.TeamName,
            teamProject.TeamManagerId
        );

        if (existingTeam == null)
        {
            LogHelper.Warning($"No team found for [{teamProject.TeamManagerId}, {teamProject.TeamName}]", log);
            throw new DomainException("No matching team found");
        }

        if (!teamProject.HasActiveProject())
        {
            throw new DomainException("At least one project must be active to associate with the team");
        }

        await AddProjectToTeamAsync(existingTeam, teamProject);
    }

    private async Task AddProjectToTeamAsync(Team existingTeam, ProjectAssociation project)
    {
        if (project.IsEmpty())
            throw new DomainException("Project association data cannot be null or empty");

        if (project.Details.Count > 3)
            throw new DomainException("A team cannot be associated with more than 3 projects");

        if (existingTeam.Project is not null)
        {
            existingTeam.Project!.AddDetail(project.Details.LastOrDefault()!);
        }
        else existingTeam.AssignProject(project);
        Console.WriteLine($"Voici le nombre de détails présent: {project.Details.Last().ProjectName}");

        await teamRepository.UpdateTeamAsync(existingTeam);
        LogHelper.Info(
            $"🔗 Team '{project.TeamName}' successfully attached to [{project.Details.Count}] project(s)",
            log
        );
    }

    public async Task SuspendProjectAsync(Guid managerId, string projectName)
    {
        var existingTeams = await teamRepository.GetTeamsByManagerIdAsync(managerId);

        var suspendedTeam = existingTeams
            .FirstOrDefault(t => t.Project != null && t.Project.Details.Any(d => d.ProjectName == projectName));

        if (suspendedTeam == null)
        {
            LogHelper.Warning($"No team found for manager {managerId} with project '{projectName}'", log);
            throw new DomainException("No matching team with the specified project found");
        }

        suspendedTeam.RemoveSuspendedProjects(projectName);

        await teamRepository.UpdateTeamAsync(suspendedTeam);

        LogHelper.Info(
            $"✅ Project '{projectName}' successfully removed from Team '{suspendedTeam.Name.Value}'",
            log
        );
    }
}
