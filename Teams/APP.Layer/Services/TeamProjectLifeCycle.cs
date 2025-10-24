using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.INFRA.Layer.Interfaces;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Humanizer;
using Teams.API.Layer.DTOs;
using AutoMapper;
using Teams.CORE.Layer.CoreServices;

namespace Teams.APP.Layer.Services;

public class TeamProjectLifeCycle(
    ITeamRepository _teamRepository,
    IUnitOfWork _unitOfWork,
    ILogger<TeamProjectLifeCycle> _log,
    IConfiguration _configuration,
    IMapper _mapper,
    TeamLifeCycleCoreService _teamLifeCycleCoreService
) : ITeamProjectLifeCycle
{
    public async Task AddProjectToTeamAsync(Team team, ProjectAssociation project)
    {
        var lastDetail = project.Details.LastOrDefault();
        if (lastDetail == null)
            throw new InvalidOperationException("Details cannot be null");

        if (team.Project is not null)
        {
            // L'équipe a déjà un projet → ajout d'un nouveau détail
            team.Project.AddDetail(lastDetail);
            Console.WriteLine($"Added new project detail '{lastDetail.ProjectName}' to existing project.");
        }
        else team.AssignProject(project);

        team.ApplyProjectAttachmentGracePeriod(_configuration.GetValue<int>("ProjectSettings:ExtraDaysBeforeExpiration"));
        await _unitOfWork.SaveAsync(CancellationToken.None); // Save changes immediately find a way to optimize this later async calls.
        BuildDto(team);
        LogHelper.Info(
           $"🔗 Team '{project.TeamName}' successfully attached to [{project.Details.Count}] project(s)",
           _log
       );
    }
    public TeamDetailsDto BuildDto(Team team)
    {
        var teamDto = _mapper.Map<TeamDetailsDto>(team);
        if (team.Project == null || team.Project.Details.Count == 0)
        {
            Console.WriteLine("No project associated with the team.");
            teamDto.HasAnyProject = false;
            teamDto.ProjectNames = null;
        }
        else
        {
            teamDto.TeamExpirationDate = team.Project.GetprojectMaxEndDate()
                                         .Value.ToInstant()
                                         .ToString("dd-MM-yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

            teamDto.HasAnyProject = true;
            teamDto.TeamManagerId = team.Project.TeamManagerId;
            teamDto.Name = team.Project.TeamName;
            teamDto.ProjectNames = team.Project.Details.Select(d => d.ProjectName).ToList();
        }
        teamDto.State = _teamLifeCycleCoreService.MatureTeam(team);
        return teamDto;
    }
    public async Task RemoveProjects(CancellationToken ct)
    {
        var teams = await _teamRepository.GetTeamsWithExpiredProject(ct);
        foreach (var team in teams)
        {
            team.RemoveExpiredProjects();
            _unitOfWork.TeamRepository.Update(team);
            LogHelper.Info($"✅ Project has been dissociated from team {team.Name}", _log);
        }
    }
    public async Task DeleteTeamProjectAsync(CancellationToken cancellationToken, Guid teamId)
    {
        var team = await _unitOfWork.TeamRepository.GetById(cancellationToken, teamId)
            ?? throw new InvalidOperationException("No matching team found");
        team.MarkAsDeleted();
        _unitOfWork.TeamRepository.Delete(team);
    }
}