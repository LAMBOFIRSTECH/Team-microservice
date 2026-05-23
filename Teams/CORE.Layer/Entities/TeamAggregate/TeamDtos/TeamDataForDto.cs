using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;

namespace Teams.CORE.Layer.Entities.TeamAggregate.TeamDtos;
// C'est le DTO intermédiaire il va regrouper ce qu'on veut pour les dtos finaux  le domaine devra tous les renvoyer 
// Ensuite on use l'auto mapper ou un assembler pour les repartir en dto finaux pour la couche APP
public record TeamDataForDto(
    Guid Id,
    string Name,
    Guid TeamManagerId,
    List<Guid> MembersIds,
    bool HasActiveProject,
    string State,
    ProjectAssociation ? Project,
    DateTimeOffset TeamCreationDate,
    DateTimeOffset TeamExpirationDate,
    double? TauxTurnOver,
    double? AverageProductivity,
    DateTimeOffset LastActivityDate,
    Guid OldTeamManagerId
)
{
    public TeamDataForDto() : this(
        Guid.Empty,
        string.Empty,
        Guid.Empty,
        new List<Guid>(),
        false,
        string.Empty,
        null,
        DateTimeOffset.MinValue,
        DateTimeOffset.MinValue,
        null,
        null,
        DateTimeOffset.MinValue,
        Guid.Empty
    )
    { }
}
