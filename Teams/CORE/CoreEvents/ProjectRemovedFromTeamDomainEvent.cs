using Teams.CORE.CoreInterfaces;
namespace Teams.CORE.CoreEvents;

/// <summary>
/// Événement de domaine immuable déclenché lorsqu'un projet est retiré d'une équipe.
/// </summary>
public sealed record ProjectRemovedFromTeamDomainEvent(Guid ProjectId, string ProjectName, Guid TeamManagerId, DateTimeOffset OccurredOn) : IDomainEvent;