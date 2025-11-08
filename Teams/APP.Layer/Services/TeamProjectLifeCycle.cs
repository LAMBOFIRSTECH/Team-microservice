using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.INFRA.Layer.Interfaces;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamExtensionMethods;
using NodatimePackage.Classes;
using Teams.API.Layer.DTOs;
using AutoMapper;
using Teams.INFRA.Layer.ExternalServicesDtos;
using FluentValidation;
using Teams.CORE.Layer.Exceptions;

namespace Teams.APP.Layer.Services;

public class TeamProjectLifeCycle(
 IUnitOfWork _unitOfWork,
 ITeamRepository _teamRepository,
 ILogger<TeamProjectLifeCycle> _log,
 IConfiguration _configuration,
 IMapper _mapper,
 IValidator<ProjectAssociationDto> _projectRecordValidator
) : ITeamProjectLifeCycle
{
    
    public string GetTimeZoneId()
    {
        var timeZoneId = _configuration.GetValue<string>("TimeZone");
        if (string.IsNullOrEmpty(timeZoneId)) throw new ArgumentNullException("Cannot get timezone id check the configuration file");
        return timeZoneId;
    }
    public DateTimeOffset PrintTimeZoneDtateTimeOffset(DateTimeOffset date) => GetTimeZoneId().ParseToLocal(date);

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

    public async Task AddProjectToTeamAsync(string message)
    {
        Console.WriteLine("dans le service projet");
        var teamProject = await GetMapProject(message);
        var existingTeam = await _teamRepository.GetTeamByNameAndTeamManagerIdAsync(teamProject.TeamName, teamProject.TeamManagerId);
        if (existingTeam == null)
        {
            LogHelper.Warning($"No team found for [{teamProject.TeamManagerId}, {teamProject.TeamName}]", _log);
            throw new InvalidOperationException("No matching team found");
        }
        if (!teamProject.HasActiveProject())
            throw new InvalidOperationException("At least one project must be active to associate with the team");

        var team = existingTeam.AddProjectToTeamExtension(teamProject);
        team.ApplyProjectAttachmentGracePeriod(_configuration.GetValue<int>("ProjectSettings:ExtraDaysBeforeExpiration"));
        await _unitOfWork.CommitAsync(CancellationToken.None);
        BuildDto(team);
        LogHelper.Info($"🔗 Team '{teamProject.TeamName}' successfully attached to [{teamProject.Details.Count}] project(s)", _log);
    }
    public async Task SuspendProjectAsync(string message)
    {
        var teamProject = await GetMapProject(message);
        var projectName = teamProject.GetprojectName();
        Console.WriteLine($"voici le project name {projectName}");
        var existingTeams = await _teamRepository.GetTeamsByManagerIdAsync(teamProject.TeamManagerId);
        var suspendedTeam = existingTeams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == teamProject.TeamManagerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (suspendedTeam == null)
            throw new NotFoundException("Team", teamProject.TeamManagerId);
        suspendedTeam.RemoveSuspendedProjects(projectName);
        _unitOfWork.TeamRepository.Update(suspendedTeam);
        LogHelper.Info($"✅ Project '{teamProject.TeamName}' successfully removed from Team '{suspendedTeam.Name.Value}'", _log);
    }
    public async Task RemoveProjects(CancellationToken ct)
    {
        var teams =  GetTeamsWithExpiredProject(ct);
        foreach (var team in teams)
        {
            team.RemoveExpiredProjects();
            _unitOfWork.TeamRepository.Update(team);
            await _unitOfWork.CommitAsync(ct);
            LogHelper.Info($"✅ Project has been dissociated from team {team.Name}", _log);
        }
    }
    public async Task DeleteTeamProjectAsync(CancellationToken cancellationToken, Guid teamId)
    {
        var team = await _unitOfWork.TeamRepository.GetById(cancellationToken, teamId) ?? throw new InvalidOperationException("No matching team found");
        team.MarkAsDeleted();
        _unitOfWork.TeamRepository.Delete(team);
    }
    public async Task<DateTimeOffset?> GetNextProjectExpirationDate(CancellationToken cancellationToken = default)
    {
        // On charge uniquement les projets et détails nécessaires
        var nextDateUtc = await _unitOfWork.Context.Teams
            .Where(t => t.Project != null)
            .SelectMany(t => t.Project!.Details)
            .Where(d => d.ProjectEndDate > TimeOperations.GetCurrentTime("UTC"))
            .OrderBy(d => d.ProjectEndDate)
            .Select(d => (DateTimeOffset?)d.ProjectEndDate)
            .FirstOrDefaultAsync(cancellationToken);

        return nextDateUtc.HasValue
            ? nextDateUtc.Value
            : null;
    }
    public List<Team> GetTeamsWithExpiredProject(CancellationToken cancellationToken = default)
    {
        var existingTeams = _unitOfWork.TeamRepository.GetAll(cancellationToken);
        var teams = existingTeams.GetTeamsWithExpiredProject();
        return teams;
    }
    public TeamDetailsDto BuildDto(Team team)
    {
        var teamDto = _mapper.Map<TeamDetailsDto>(team);
        if (team.Project == null || team.Project.Details.Count == 0)
        {
            teamDto.HasAnyProject = false;
            teamDto.ProjectNames = null;
        }
        else
        {
            var teamExpiration = team.TeamExpirationDate;
            var projetMaxEndDate = team.Project?.GetprojectMaxEndDate() ?? teamExpiration;
            DateTimeOffset maxDateUtc = projetMaxEndDate > teamExpiration ? projetMaxEndDate : teamExpiration;
            var localMaxDate = maxDateUtc.ToString("dd-MM-yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            teamDto.TeamExpirationDate = localMaxDate;
            teamDto.HasAnyProject = true;
            teamDto.TeamManagerId = team.Project!.TeamManagerId;
            teamDto.Name = team.Project.TeamName;
            teamDto.ProjectNames = team.Project.Details.Select(d => d.ProjectName).ToList();
        }
        teamDto.State = team.MatureTeam();
        return teamDto;
    }
}