using MediatR;
namespace Teams.APP.Layer.FeatureTeam.DeleteTeam;
public record DeleteTeamCommand(Guid Id, string Name) : IRequest;
