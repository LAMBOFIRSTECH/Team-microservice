namespace Teams.APP.Layer.DTOs;

public class TeamStatsDto
{
    public Guid Id { get; set; }
    public DateTimeOffset LastActivityDate { get; set; }
    public double? TauxTurnOver { get; set; }
    public double? AverageProductivity { get; set; }
    public TeamStatsDto()
    {
        TauxTurnOver = null;
        AverageProductivity = null;
        LastActivityDate = DateTimeOffset.MinValue;
    }
    public TeamStatsDto(
        double taux,
        double avg,
        DateTimeOffset dateTime
    )
    {
        TauxTurnOver = taux;
        AverageProductivity = avg;
        LastActivityDate = dateTime;
    }
}