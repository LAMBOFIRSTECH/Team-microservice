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
    public Guid Id { get; }
    public Guid TeamManagerId { get; }
    public string Name { get; set; } = string.Empty;
    public List<Guid> MemberId { get; set; }
    public GetAllTeamsQuery()
    {
        Id =
        TeamManagerId = Guid.Empty;
        MemberId = new List<Guid>();
        Name = string.Empty;
    }
    public GetAllTeamsQuery(Guid identifier, Guid teamManagerId, List<Guid> memberId, string name = "")
    {
        this.Id = identifier;
        this.TeamManagerId = teamManagerId;
        this.MemberId = memberId;
        this.Name = name;
    }
}