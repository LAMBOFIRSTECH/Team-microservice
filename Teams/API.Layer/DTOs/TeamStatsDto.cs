namespace Teams.API.Layer.DTOs;

public class TeamStatsDto
{
    public Guid Id { get; set; }
    public DateTime LastActivityDate { get; set; }
    public double? TauxTurnOver { get; set; }
    public double? AverageProductivity { get; set; }
    public TeamStatsDto() { }
    public TeamStatsDto(
        double taux,
        double avg,
        DateTime dateTime
    )
    {
        TauxTurnOver = taux;
        AverageProductivity = avg;
        LastActivityDate = dateTime;
    }
}