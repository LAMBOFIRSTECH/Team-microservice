using AutoMapper;
using FluentValidation;
using MediatR;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.CoreEvents;
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
    IMapper mapper,
    IMediator _mediator
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
            LogHelper.Warning(
                $"Team {teamProject.TeamName} already has an active project name's {teamProject.ProjectName}.",
                log
            );
            throw new DomainException(
                $"Team {teamProject.TeamName} already has an active project."
            );
        }
        existingTeam.AttachProjectToTeam(teamProject, true);
        await teamRepository.UpdateTeamAsync(existingTeam);
        LogHelper.Info(
            $"🔗📁👥 Team {teamProject.TeamName} has been attached to [{teamProject.ProjectName}] project successfully.",
            log
        );
        await SendProjectNotification(teamProject);
    }

    public async Task SendProjectNotification(ProjectAssociation projectAssociation) =>
        await _mediator.Publish(
            new ProjectAssociatedNotification(
                projectAssociation.ProjectName,
                projectAssociation.ProjectEndDate,
                $"Team {projectAssociation.TeamName} and project {projectAssociation.ProjectName} associated at {DateTime.UtcNow}, for manager {projectAssociation.TeamManagerId}."
            )
        );
}
