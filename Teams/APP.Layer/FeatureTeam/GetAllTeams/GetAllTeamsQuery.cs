using MediatR;
using Teams.APP.Layer.DTOs.Output;
namespace Teams.APP.Layer.FeatureTeam.GetAllTeams;

public sealed class GetAllTeamsQuery : IRequest<List<TeamDtoModels.TeamDto>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllTeamsQuery"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is parameterless and is used for deserialization purposes.
    /// It allows the MediatR library to create instances of this query without requiring any parameters.
    /// </remarks>
    public Guid Id { get; init;}
    public Guid TeamManagerId { get; init;}
    public string Name { get; init; }
    public IEnumerable<Guid> MemberId { get; init; }

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
        Id = identifier;
        TeamManagerId = teamManagerId;
        MemberId = memberId;
        Name = name;
        OnlyMature = onlyMature;
    }
}
