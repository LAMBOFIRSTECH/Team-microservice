using MediatR;
using Teams.API.Layer.DTOs;
namespace Teams.APP.Layer.CQRS.Queries;
public class GetAllTeamsQuery : IRequest<TeamDto>
{
    public Guid Id { get; }

    public GetAllTeamsQuery(Guid identifier)
    {
        Id = identifier;
    }
}
