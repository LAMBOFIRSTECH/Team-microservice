using Teams.CORE.Layer.Entities;
using Teams.INFRA.Layer.ExternalServicesDtos;
using Teams.CORE.Layer.ValueObjects;
namespace Teams.API.Layer.DTOs;


public class TeamDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string TeamCreationDate { get; set; }
    public string TeamExpirationDate { get; set; }
    public bool ActiveProject { get; set; }
    public List<string>? ProjectNames { get; set; }

    public string State { get; set; }
#pragma warning disable CS8618 
    public TeamDetailsDto() { }
#pragma warning restore CS8618 
    public TeamDetailsDto(
        string teamName,
        DateTime teamCreationDate,
        DateTime teamExpirationDate,
        TeamState state,
        List<string> projectNames,
        bool activeProject
    )
    {
        Name = teamName;
        ProjectNames = projectNames;
        State = Enum.GetName(state.GetType(), state)!;
        ActiveProject = activeProject;
        TeamCreationDate = ReadableDateTimeFormat(teamCreationDate);
        TeamExpirationDate = ReadableDateTimeFormat(teamExpirationDate);

    }
    private string ReadableDateTimeFormat(DateTime dt) => dt.ToString("dd-MM-yyyy",
                                   System.Globalization.CultureInfo.InvariantCulture);

}