using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.INFRA.Layer.Interfaces;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.CommonExtensions;
using NodatimePackage.Classes;
using Teams.API.Layer.DTOs;
using AutoMapper;

namespace Teams.APP.Layer.Services;

public class TeamProjectLifeCycle(IUnitOfWork _unitOfWork, ILogger<TeamProjectLifeCycle> _log, IConfiguration _configuration, IMapper _mapper) : ITeamProjectLifeCycle
{
    public string GetTimeZoneId()
    {
        var timeZoneId = _configuration.GetValue<string>("TimeZone");
        if (string.IsNullOrEmpty(timeZoneId)) throw new ArgumentNullException("Cannot get timezone id check the configuration file");
        return timeZoneId;
    }
    public DateTimeOffset PrintTimeZoneDtateTimeOffset(DateTimeOffset date) => GetTimeZoneId().ParseToLocal(date);
    public async Task AddProjectToTeamAsync(Team team, ProjectAssociation project)
    {
        Console.WriteLine("dans la fonction d'ajout de projet de team projet lifecycle");
        if (project == null)
            throw new ArgumentNullException(nameof(project), "ProjectAssociation cannot be null");

        if (project.Details == null || !project.Details.Any())
            throw new InvalidOperationException("ProjectAssociation must contain at least one Detail");

        if (team.Project != null) foreach (var detail in project.Details) team.Project.AddDetail(detail);
        else team.AssignProject(project);

        team.ApplyProjectAttachmentGracePeriod(_configuration.GetValue<int>("ProjectSettings:ExtraDaysBeforeExpiration"));
        await _unitOfWork.SaveAsync(CancellationToken.None);
        await BuildDto(team);
        LogHelper.Info($"🔗 Team '{project.TeamName}' successfully attached to [{project.Details.Count}] project(s)", _log);
    }
    public async Task RemoveProjects(CancellationToken ct)
    {
        var teams = await GetTeamsWithExpiredProject(ct);
        foreach (var team in teams)
        {
            team.RemoveExpiredProjects();
            _unitOfWork.TeamRepository.Update(team);
            await _unitOfWork.SaveAsync(ct);
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
        // Charger uniquement les projets et détails nécessaires
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
    public async Task<List<Team>> GetTeamsWithExpiredProject(CancellationToken cancellationToken = default)
    {
        var existingTeams = _unitOfWork.TeamRepository.GetAll(cancellationToken);
        var teams = existingTeams.GetTeamsWithExpiredProject();
        return teams;
    }
    public async Task<TeamDetailsDto> BuildDto(Team team)
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