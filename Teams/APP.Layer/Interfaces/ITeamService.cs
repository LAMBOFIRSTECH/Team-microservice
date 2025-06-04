// - Gère l'interaction entre les couhes ( c'est l'orchestrateur)
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.CQRS.Commands;
namespace Teams.APP.Layer.Interfaces;
public interface ITeamService
{
    Task CreateTeamAsync(CreateTeamCommand command);
    Task<TeamDto> GetTeamAsync(Guid identifier);
}
