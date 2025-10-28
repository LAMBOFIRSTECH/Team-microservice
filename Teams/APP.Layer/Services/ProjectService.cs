using AutoMapper;
using FluentValidation;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.CoreServices;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.ExternalServices;
using Teams.INFRA.Layer.ExternalServicesDtos;
using Teams.INFRA.Layer.Interfaces;
using Teams.CORE.Layer.CommonExtensions;

namespace Teams.APP.Layer.Services;

public class ProjectService(
    ITeamRepository _teamRepository,
    TeamExternalService _teamExternalService,
    IUnitOfWork _unitOfWork,
    ProjectLifeCycle _projectLifeCycleCore,
    ITeamProjectLifeCycle _teamProjectLifeCycle,
    ILogger<ProjectService> _log,
    IValidator<ProjectAssociationDto> _projectRecordValidator,
    IMapper _mapper
) : IProjectService
{
    public async Task<ProjectAssociation> GetProjectAssociationDataAsync(Guid? managerId, string teamName)
    {
        var dto = await _teamExternalService.RetrieveProjectAssociationDataAsync();
        if (dto == null)
        {
            LogHelper.Error("❌ Failed to retrieve project association data", _log);
            throw new InvalidOperationException("Failed to retrieve project association data");
        }

        if (dto.TeamManagerId != managerId || dto.TeamName != teamName)
        {
            LogHelper.Error(
                $"❌ Mismatch: Expected [{managerId}, {teamName}], Received [{dto.TeamManagerId}, {dto.TeamName}]",
                _log
            );
            throw new InvalidOperationException("Mismatched team manager or team name");
        }

        var validationResult = await _projectRecordValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            LogHelper.CriticalFailure(_log, "Data validation", $"{validationResult}", null);
            throw new InvalidOperationException("Project association data is invalid");
        }

        return _mapper.Map<ProjectAssociation>(dto);
    }
    public async Task ManageTeamProjectAsync(Guid managerId, string teamName)
    {
        var teamProject = await GetProjectAssociationDataAsync(managerId, teamName);
        var existingTeam = await _teamRepository.GetTeamByNameAndTeamManagerIdAsync(teamProject.TeamName, teamProject.TeamManagerId);
        if (existingTeam == null)
        {
            LogHelper.Warning($"No team found for [{teamProject.TeamManagerId}, {teamProject.TeamName}]", _log);
            throw new InvalidOperationException("No matching team found");
        }
        if (!teamProject.HasActiveProject())
            throw new InvalidOperationException("At least one project must be active to associate with the team");

        await _teamProjectLifeCycle.AddProjectToTeamAsync(existingTeam, teamProject);


    }
    public async Task SuspendProjectAsync(Guid managerId, string projectName)
    {
        var existingTeams = await _teamRepository.GetTeamsByManagerIdAsync(managerId);
        var suspendedTeam = await _projectLifeCycleCore.SuspendProjectAsync(managerId, projectName, existingTeams);
        _unitOfWork.TeamRepository.Update(suspendedTeam);
        LogHelper.Info(
            $"✅ Project '{projectName}' successfully removed from Team '{suspendedTeam.Name.Value}'",
            _log
        );
    }


}
