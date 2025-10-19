namespace Teams.APP.Layer.Interfaces;

public interface ITeamProjectLifeCycle
{
    Task RemoveProjects(CancellationToken ct);
}