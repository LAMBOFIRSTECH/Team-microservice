using MediatR;
using Teams.API.Layer.DTOs;

namespace Teams.APP.Layer.CQRS.Queries;

public class GetTeamQuery : IRequest<TeamDetailsDto>
{
    public Guid Id { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTeamQuery"/> class with the specified team identifier.
    /// </summary>
    /// <param name="identifier">The unique identifier of the team to retrieve.</param>
    public GetTeamQuery(Guid identifier)
    {
        Id = identifier;
    }
}
