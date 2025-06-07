using MediatR;
using Teams.API.Layer.DTOs;
namespace Teams.APP.Layer.CQRS.Queries;
public class GetAllTeamsQuery : IRequest<List<TeamDto>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllTeamsQuery"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is parameterless and is used for deserialization purposes.
    /// It allows the MediatR library to create instances of this query without requiring any parameters.
    /// </remarks>
}