namespace Teams.CORE.Errors;

/// <summary>
/// Contrat minimal que toute exception "métier connue" (par opposition à un
/// bug système imprévu) doit respecter pour être traduite automatiquement
/// par n'importe quelle couche Presentation. Volontairement réduit à deux
/// propriétés — pas de StatusCode, pas de nom de header, rien qui présuppose
/// un protocole de transport.
/// </summary>
public interface IHasErrorNature
{
    ErrorNature Nature { get; }

    /// <summary>
    /// Code court, stable, machine-readable (ex: "team_archived",
    /// "member_rest_period"). Sert de clé de traduction i18n côté client,
    /// ou de valeur de log structuré — jamais affiché tel quel à l'utilisateur.
    /// </summary>
    string Reason { get; }
}