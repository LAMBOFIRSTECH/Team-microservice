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
    public string Name { get; set; }
    public IEnumerable<Guid> MemberId { get; set; }

    /// <summary>
    /// Si true, ne renvoie que les équipes matures
    /// </summary>
    public bool OnlyMature { get; set; } = false;

    public GetAllTeamsQuery()
    {
        Id = TeamManagerId = Guid.Empty;
        MemberId = new List<Guid>();
        Name = string.Empty;
    }

    public GetAllTeamsQuery(
        Guid identifier,
        Guid teamManagerId,
        IEnumerable<Guid> memberId,
        string name = "",
        bool onlyMature = false
    )
    {
        this.Id = identifier;
        this.TeamManagerId = teamManagerId;
        this.MemberId = memberId;
        this.Name = name;
        this.OnlyMature = onlyMature;
    }
}
