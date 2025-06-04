using MediatR;
using Teams.API.Layer.DTOs;
namespace Teams.APP.Layer.CQRS.Queries;
public class GetTeamQuery : IRequest<TeamDto>
{
    public Guid Id { get; }

    public GetTeamQuery(Guid identifier)
    {
        Id = identifier;
    }
}
