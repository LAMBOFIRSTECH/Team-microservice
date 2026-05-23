namespace Teams.APP.Layer.FeatureTeam.UpdateTeamByManager;
public record UpdateTeamByManagerRequest(string TeamName, Guid OldTeamManagerId, Guid NewTeamManagerId, string ContratType);