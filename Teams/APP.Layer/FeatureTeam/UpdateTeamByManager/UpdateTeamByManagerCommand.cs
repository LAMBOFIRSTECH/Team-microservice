using MediatR;
namespace Teams.APP.Layer.FeatureTeam.UpdateTeamByManager;
public record UpdateTeamByManagerCommand(string TeamName, Guid OldTeamManagerId, Guid NewTeamManagerId, string ContratType) : IRequest<Unit>;

