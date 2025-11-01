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
    IUnitOfWork _unitOfWork,
    ProjectLifeCycle _projectLifeCycleCore,
    ITeamProjectLifeCycle _teamProjectLifeCycle,
    ILogger<ProjectService> _log,
    IValidator<ProjectAssociationDto> _projectRecordValidator,
    IConfiguration _configuration,
    IMapper _mapper
) : IProjectService
{
    public string GetTimeZoneId()
    {
        var timeZoneId = _configuration.GetValue<string>("TimeZone");
        if (string.IsNullOrEmpty(timeZoneId)) throw new ArgumentNullException("Cannot get timezone id check the configuration file");
        return timeZoneId;
    }
    public async Task<ProjectAssociation> GetMapProject(string message)
    {
        var dto = await message.GetDtoConverted<ProjectAssociationDto>();
        var validationResult = await _projectRecordValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            LogHelper.CriticalFailure(_log, "Data validation", $"{validationResult}", null);
            throw new InvalidOperationException("Project association data is invalid");
        }
        return _mapper.Map<ProjectAssociation>(dto);
    }

    public async Task ProjectAssociationDataAsync(string message)
    {
        Console.WriteLine("danas le service projet");
        var teamProject = await GetMapProject(message);
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
    public async Task SuspendProjectAsync(string message)
    {
        var teamProject = await GetMapProject(message);
        var existingTeams = await _teamRepository.GetTeamsByManagerIdAsync(teamProject.TeamManagerId);
        var suspendedTeam = await _projectLifeCycleCore.SuspendProjectAsync(teamProject.TeamManagerId, teamProject.TeamName, existingTeams); // a revoir pourquoi use la liste de team au lieu de l'équipe seule
        _unitOfWork.TeamRepository.Update(suspendedTeam);
        LogHelper.Info($"✅ Project '{teamProject.TeamName}' successfully removed from Team '{suspendedTeam.Name.Value}'", _log);
    }
}
