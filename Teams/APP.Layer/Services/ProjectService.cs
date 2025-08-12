using AutoMapper;
using FluentValidation;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.BusinessExceptions;
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
)
{
    public async Task ManageTeamteamProjectAsync()
    {
        var dto = await teamExternalService.RetrieveProjectAssociationDataAsync();
        var validationResult = await projectRecordValidator.ValidateAsync(dto!);
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
        existingTeam.AttachProjectToTeam(teamProject, true);
        await teamRepository.UpdateTeamAsync(existingTeam);
        LogHelper.Info(
            $"🔗📁👥 Team {teamProject.TeamName} has been attached to [{teamProject.ProjectName}] project successfully.",
            log
        );
    }
}
