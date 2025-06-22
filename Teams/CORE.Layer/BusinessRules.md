# Les règles métier pour l'entité Equipe

`EmployeeService` et `ProjectService` (sont des services à part qui devront etre injecter dans le handler)

1. Règles de validation de données (Fluent validation)
--------------------------------------------------------------------------------------------------------
| Règle                                           | Exemple                                            |
| ----------------------------------------------- | ---------------------------------------------------|
|☑️  Une équipe doit avoir un nom non vide        | `"NomEquipe != null && NomEquipe.Length > 0"`      | 
|                                                 |                                                    |
| Le responsable doit être un employé valide      | Vérifié via `EmployeeService`                       |
|                                                 |                                                    |
| Les membres doivent être des employés existants | Vérifié avant ajout                                |
--------------------------------------------------------------------------------------------------------


2. Règles d’invariant métier
-------------------------------------------------------------------------------------------------------------------
| Règle                                                           | Invariant                                     |
| --------------------------------------------------------------- | ----------------------------------------------|
|☑️ Une équipe doit toujours avoir **un et un seul responsable** | `Equipe.Responsable != null`                   |
|                                                                 |                                               |
|☑️ Un responsable **doit être membre** de l’équipe              | `Equipe.Membres.Contains(Responsable)`         |
|                                                                 |                                                |
|☑️ Un employé ne peut pas apparaître deux fois dans la même équipe | `Membres.Distinct().Count == Membres.Count` |
--------------------------------------------------------------------------------------------------------------------


3. Règles d’agrégation métier
-------------------------------------------------------------------------------------------
| Règle                                          | Exemple                                |
|------------------------------------------------|--------------------------------------- |
|☑️Une équipe ne peut pas dépasser 10 membres    | `if (Membres.Count >= 10) throw ...`   |
|                                                |                                         |
|☑️Minimum 2 membres pour valider une équipe     | Empêche les équipes “fantômes”         |
-------------------------------------------------------------------------------------------


4. Règles de workflow / transition d’état
-----------------------------------------------------------------------------------------------------------------------------------------------
| Règle                                                                              | Exemple                                                 |
|------------------------------------------------------------------------------------|---------------------------------------------------------|
| Une équipe ne peut être activée que si elle a un responsable et au moins un membre | `Etat == Actif` validé à l’activation                   |
|                                                                                    |                                                         |
| Une équipe archivée ne peut plus être modifiée                                     | `if (Equipe.Etat == Archivee) throw BusinessException`  |
|                                                                                    |                                                         |
| Suppression d’une équipe possible uniquement si aucun projet en dépend             | `ProjectService.HasNoDependencies(equipeId)`            |
|                                                                                    |                                                         |
| Changer de responsable est possible seulement si le nouveau est déjà membre        | Pas de promotion externe directe                        |
------------------------------------------------------------------------------------------------------------------------------------------------

5. Règles de calcul métier
-------------------------------------------------------------------------------------------------------------
| Règle                                   | Exemple                                                         |
|---------------------------------------- | ----------------------------------------------------------------|
| Calcul du nombre de membres             | `Membres.Count()`                                               |
|                                         |                                                                 |
| Score de stabilité d’équipe             | Basé sur turnover des membres (moyenne des jours dans l’équipe) |
|                                         |                                                                 |
| Durée moyenne de présence des membres   | `Avg(DateTime.Now - membre.JoinDate)`                           |
|                                         |                                                                 |
| Taux moyen de productivité d’une équipe | Moyenne des scores des membres (via Activités)                  |
-------------------------------------------------------------------------------------------------------------


6. Règles de dérivation métier
---------------------------------------------------------------------------------------------------------------------
| Règle                                   | Exemple                                                                 |
|-----------------------------------------|-------------------------------------------------------------------------|
|Statut de l’équipe (Complète /Incomplète)|                                                                         |
|dérivé du nombre de membres et de        | `Etat = (Membres.Count >= 3 && Responsable != null) ? Actif : Incomplet`| 
|la présence d’un responsable             |                                                                         |
|                                         |                                                                         |
| Taux d'engagement de l’équipe           | % de membres actifs dans des projets                                    |
|                                         |                                                                         |
| Équipe "mature" si > 6 mois d'existence | `DateTime.Now - DateCreation >= 180 jours`                              |
---------------------------------------------------------------------------------------------------------------------


7. Règles d’accès métier / autorisation
---------------------------------------------------------------------------------------------------------------------------
| Règle                                                                              | Exemple                            |
|------------------------------------------------------------------------------------|------------------------------------|
|☑️Seul un responsable peut modifier les membres                                     | Autorisation métier, non technique |
|                                                                                     |                                   |
|☑️ Seuls les administrateurs peuvent supprimer une équipe                           | Protection de la suppression       |
|                                                                                    |                                     |
|☑️ Seuls les admins ou les managers peuvent ajouter un membre dans une équipe       | Protection de l'ajout              |
|                                                                                    |                                     |
|☑️ Seul un membre qui n'est pas un **team manager** peut être supprimé d'une équipe | Protection de la suppression       |
---------------------------------------------------------------------------------------------------------------------------



8. Règles temporelles / périodiques (dictionnaire ou cache dans l'implémentation)
---------------------------------------------------------------------------------------------------------------------------------------------------
| Règle                                                                                                    | Exemple                               |
|--------------------------------------------------------------------------------------------------------- | --------------------------------------|
|☑️ Un membre ne peut pas être ajouté à une équipe **s’il en a quitté une autre il y a moins de 7 jours** | Délai de “repos” entre deux équipes   |
|                                                                                                          |                                       |
| Une équipe inactive depuis plus de 90 jours est archivée automatiquement                                 | Via job planifié dans `EquipeService` |
----------------------------------------------------------------------------------------------------------------------------------------------------


9.  Règles d’intégrité métier
---------------------------------------------------------------------------------------------------------
| Règle                                                   | Détails                                     |
|---------------------------------------------------------| --------------------------------------------|
| Tous les membres doivent exister dans `EmployeeService` | Validé à l’ajout                            |
|                                                         |                                             |
| Une équipe ne peut être affectée à un projet inexistant | `ProjectService` doit valider l’affectation |
|                                                         |                                             |
| Les ID des membres doivent être valides et uniques      | GUID ou Id interne valide                   |
---------------------------------------------------------------------------------------------------------


10. Règles de dépendance / prérequis
-----------------------------------------------------------------------------------------------------------------------------------
| Règle                                                               | Exemple                                                   |
|-------------------------------------------------------------------- | --------------------------------------------------------- |
| Une équipe ne peut être affectée à un projet que si elle est active | `ProjetService.ValiderAffectation(EquipeId)`              |
|                                                                     |                                                           |
| Un projet ne peut commencer que si au moins une équipe est affectée | Règle côté `ProjetService` mais dépend de `EquipeService` |
-----------------------------------------------------------------------------------------------------------------------------------

11. Règles de cohérence transversales
--------------------------------------------------------------------------------------------------------------------------------------
| Règle                                                                   | Exemple                                                  |
| ----------------------------------------------------------------------- | -------------------------------------------------------- |
| Une équipe ne peut avoir un nom déjà utilisé                            | `TeamRepository.NameAlreadyExists(name)`                 |
|                                                                         |                                                          |
| Le responsable ne peut être supprimé tant qu’il n’est pas remplacé      | `if (membreId == ResponsableId) throw BusinessException` |
|                                                                         |                                                          |
| Interdiction d’avoir deux équipes avec **exactement** les mêmes membres | Empêche duplication structurelle                         |
--------------------------------------------------------------------------------------------------------------------------------------
