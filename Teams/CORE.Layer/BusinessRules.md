# Les règles métier pour l'entité Equipe

`EmployeeService` et `ProjectService` (sont des services à part qui devront etre injecter dans le handler)

-------------------------------------------------------------------------------------------------------
| Aggregate Root | Entités enfants                    | Invariants à protéger                         |
| -------------- | ---------------------------------- | --------------------------------------------- |
| `Equipe`       | `Membres`, `Responsable`, `Statut` | Taille, unicité, règles de rôle               |
| `Projet`       | `Tâches`, `Compétences attendues`  | État projet, affectation équipes, dépendances |
| `Employé`      | `Compétences`, `Historique`, etc.  | Affectation, disponibilité, type de contrat   |
--------------------------------------------------------------------------------------------------------

Les differents Etat d'une équipe et leur signification
---------------------------------------------------------------------------------------------------------------------------------------
|Statut (Team)   | Description                                                       | Déclencheurs / Conditions métier               |
|----------------|-------------------------------------------------------------------|----------------------------------------------- |
|**Draft**       |Équipe en cours de création (moins de 3 membres ou pas de manager) |`Membres.Count < 3 ou Responsable == null`      |
|                |                                                                   |                                                |
|**Active**      |Équipe complète (≥ 3 membres + 1 manager)                          |`Membres.Count >= 3` et `Responsable != null`   |
|                |                                                                   |                                                |
|**Archived**    |Équipe inactive ou restée non valide après un certain délai        |`State = TeamState.Archived`                    |
|-------------------------------------------------------------------------------------------------------------------------------------|

|**************************|***********************************************|************************************************|
|Statut (Project)          | Description                                   | Déclencheurs / Conditions métier               |
|--------------------------|-----------------------------------------------|------------------------------------------------|
|**Unaffected**            |Aucune affectation projet                      |`ProjectAssignmentState.Unassigned`             | 
|                          |                                               |                                                |
|**Assigned**              |Projet en cours                                | `ProjectAssignmentState.Assigned`              |
|                          |                                               |                                                |
|**Suspended**             |Projet(s) associé(s) suspendu(s)               |`ProjectAssignmentState.Suspended`              |
|                          |                                               |                                                |
|**UnderReview**           |Projet en cours d’évaluation pour réaffectation|`ProjectAssignmentState.UnderReview`            |
|                          |                                               |                                                |
|**UnassignedAfterReview** |Équipe restée sans projet après révision       |`ProjectAssignmentState.UnassignedAfterReview`  |
|---------------------------------------------------------------------------------------------------------------------------|


# Entité Equipe microservice dédié

## A- Richesse fonctionnelle et intéraction avec le système

---------------------------------------------------------------------------------------
| Interactions           | Rôle                                                       |
| ---------------------- | ---------------------------------------------------------- |
| `EmployeeService`      | Pour valider les employés lors de l’ajout                  |
|                        |                                                            |
| `ProjectService`       | Pour affecter une équipe à un projet                       |
|                        |                                                            |
| `FicheActiviteService` | Pour voir les activités par équipe                         |
|                        |                                                            |
| `NotificationService`  | Envoi d’email aux membres/responsable en cas de changement |
---------------------------------------------------------------------------------------

## B- Evènements métier (Even dispatching)

------------------------------------------------------------------------------------------------
| Event                      | Payload principal              | Destinataires                  |
|----------------------------|--------------------------------|--------------------------------|
| `EquipeCreee`              | Id, Responsable, MembreIds     | Notification, RH               |
|                            |                                |                                |
| `MembreAjouteAÉquipe`      | IdEquipe, MembreId             | EmployeService                 |
|                            |                                |                                |
| `MembreRetireDEquipe`      | IdEquipe, MembreId             | EmployeService                 |
|                            |                                |                                |
| `ResponsableEquipeModifie` | EquipeId, OldRespId, NewRespId | ProjetService                  |
|                            |                                |                                |
| `EquipeAffecteeAProjet`    | ProjetId, EquipeId             | ProjetService, ActiviteService |
|                            |                                |                                |
| `EquipeSupprimee`          | EquipeId                       | Tous les services dépendants   |
------------------------------------------------------------------------------------------------

## C- Agrégats et règles


1. Règles de validation de données (Fluent validation)
-----------------------------------------------------------------------------------------------------------
| Règle                                              | Exemple                                            |
| -------------------------------------------------- | ---------------------------------------------------|
|☑️ Une équipe doit avoir un nom non vide           | `"NomEquipe != null && NomEquipe.Length > 0"`       | 
|                                                    |                                                    |
|☑️ Le responsable doit être un employé valide      | Vérifié via `EmployeeService`                       |
|                                                    |                                                    |
|☑️ Les membres doivent être des employés existants | Vérifié avant ajout                                 |
-----------------------------------------------------------------------------------------------------------


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
|☑️Minimum 3 membres pour valider une équipe     | Empêche les équipes “fantômes”         |
-------------------------------------------------------------------------------------------


4. Règles de workflow / transition d’état
-----------------------------------------------------------------------------------------------------------------------------------------------
| Règle                                                                              | Exemple                                                 |
|------------------------------------------------------------------------------------|---------------------------------------------------------|
|☑️ Une équipe ne peut être activée que si elle a un responsable et au moins 3 membres |`Etat == Actif` validé à l’activation(Redis ID + status) |
|                                                                                    |                                                         |
|☑️ Une équipe archivée ne peut plus être modifiée                                  | `if (Equipe.Etat == Archivee) throw BusinessException`  |
|                                                                                    |                                                         |
|☑️ Suppression d’une équipe possible uniquement si aucun projet en dépend          | `ProjectService.HasNoDependencies(equipeId)`            |
|                                                                                    |                                                         |
|☑️ Changer de responsable est possible seulement si le nouveau est déjà membre      | Pas de promotion externe directe                        |
------------------------------------------------------------------------------------------------------------------------------------------------


5. Règles de calcul métier
-----------------------------------------------------------------------------------------------------------------------------------------
| Règle                                                               | Exemple                                                         |
|---------------------------------------------------------------------| ----------------------------------------------------------------|
| Calcul de productivité démarre à cette date de lancement du projet  | Moyenne des scores des membres (via Activités)                  |
|                                                                     |                                                                 |
| ☑️ Calcul du nombre de membres                                         | `Membres.Count()`                                               |
|                                                                     |                                                                 |
| Score de stabilité d’équipe                                         | Basé sur turnover des membres (moyenne des jours dans l’équipe) |
|                                                                     |                                                                 |
| Durée moyenne de présence des membres                               | `Avg(DateTime.Now - membre.JoinDate)`                           |
|                                                                     |                                                                 |
| Taux moyen de productivité d’une équipe                             | Moyenne des scores des membres (via Activités)                  |
-----------------------------------------------------------------------------------------------------------------------------------------


6. Règles de dérivation métier
-------------------------------------------------------------------------------------------------------------------------
| Règle                                       | Exemple                                                                 |
|---------------------------------------------|-------------------------------------------------------------------------|
|☑️ Statut de l’équipe (Complète /Incomplète)|                                                                          |
|dérivé du nombre de membres et de            | `Etat = (Membres.Count >= 3 && Responsable != null) ? Actif : Incomplet`| 
|la présence d’un responsable                 |                                                                         |
|                                             |                                                                         |
| Taux d'engagement de l’équipe               | % de membres actifs dans des projets                                    |
|                                             |                                                                         |
|☑️ Équipe "mature" si > 6 mois d'existence   | `DateTime.Now - DateCreation >= 180 jours`                              |
-------------------------------------------------------------------------------------------------------------------------


7. Règles d’accès métier / autorisation
----------------------------------------------------------------------------------------------------------------------------
| Règle                                                                               | Exemple                            |
|-------------------------------------------------------------------------------------|------------------------------------|
|☑️ Seul un responsable peut modifier les membres                                     | Autorisation métier, non technique |
|                                                                                     |                                    |
|☑️ Seuls les administrateurs peuvent supprimer une équipe                            | Protection de la suppression       |
|                                                                                     |                                     |
|☑️ Seuls les admins ou les managers peuvent ajouter un membre dans une équipe        | Protection de l'ajout              |
|                                                                                     |                                     |
|☑️ Seul un membre qui n'est pas un **team manager** peut être supprimé d'une équipe  | Protection de la suppression       |
-----------------------------------------------------------------------------------------------------------------------------


8. Règles temporelles / périodiques (dictionnaire ou cache dans l'implémentation)
---------------------------------------------------------------------------------------------------------------------------------------------------
| Règle                                                                                                    | Exemple                               |
|--------------------------------------------------------------------------------------------------------- | --------------------------------------|
|☑️ Un membre ne peut pas être ajouté à une équipe **s’il en a quitté une autre il y a moins de 7 jours**  | Délai de “repos” entre deux équipes   |
|                                                                                                          |                                       |
|☑️ Une équipe inactive depuis plus de 90 jours est archivée automatiquement                               | Via job planifié dans `EquipeService` |
|                                                                                                          |                                       |
| Un membre ne peut pas rester plus de N mois sans changer d’équipe. Eviter l’enfermement                  |                                       |
|                                                                                                          |                                       |
|☑️ Le délai entre création et lancement du projet ne doit pas dépasser 7 jours                            |                                       |
----------------------------------------------------------------------------------------------------------------------------------------------------


9.  Règles d’intégrité métier
-----------------------------------------------------------------------------------------------------------
| Règle                                                     | Détails                                     |
|-----------------------------------------------------------| --------------------------------------------|
|☑️ Tous les membres doivent exister dans `EmployeeService` | Validé à l’ajout                            |
|                                                           |                                             |
|☑️ Une équipe ne peut être affectée à un projet inexistant | `ProjectService` doit valider l’affectation |
|                                                            |                                             |
|☑️ Les ID des membres doivent être valides et uniques      | GUID ou Id interne valide                    |
------------------------------------------------------------------------------------------------------------


10. Règles de dépendance / prérequis
-----------------------------------------------------------------------------------------------------------------------------------
| Règle                                                               | Exemple                                                   |
|-------------------------------------------------------------------- | --------------------------------------------------------- |
|☑️ Une équipe ne peut être affectée à un projet que si elle est active | `ProjetService.ValiderAffectation(EquipeId)`              |
|                                                                     |                                                           |
|☑️ Un projet ne peut commencer que si au moins une équipe est affectée | Règle côté `ProjetService` mais dépend de `EquipeService` |
-----------------------------------------------------------------------------------------------------------------------------------


11. Règles de cohérence transversales
---------------------------------------------------------------------------------------------------------------------------------------------------------
| Règle                                                                   | Exemple                                                                      |
| ----------------------------------------------------------------------- | -----------------------------------------------------------------------------|
|☑️ Une équipe ne peut avoir un nom déjà utilisé                          | `TeamRepository.NameAlreadyExists(name)`                                     |
|                                                                         |                                                                              |
|☑️ Le responsable ne peut être supprimé tant qu’il n’est pas remplacé    | `if (membreId == ResponsableId) throw BusinessException`                    |
|                                                                         |                                                                              |
|☑️ Interdiction d’avoir deux équipes avec **exactement** les mêmes membres |Empêche duplication structurelle `(GetAllTeamsQueryHandler)`               |
|                                                                         |                                                                              |
|☑️ Une équipe-associée-projet ne peut etre supprimée if `ExpirationDate > ValidityPeriodInDays `| update la date expiration de 30 jours                   |
---------------------------------------------------------------------------------------------------------------------------------------------------------


12. Règles de performance 
------------------------------------------------------------------------------------------------------------------------------------
| Règle                                                                           | Exemple                                        |
| ------------------------------------------------------------------------------- | ---------------------------------------------- |
| Une équipe ayant un taux de productivité moyen < 40% sur 3 mois doit être revue | Détection automatique via indicateurs          |
|                                                                                 |                                                |
| Trop de turn-over (> 50% en 2 mois) déclenche une alerte au RH                  | Extrait des `Join/LeaveDate` dans l’historique |
------------------------------------------------------------------------------------------------------------------------------------


13. Règles de cycle de vie projet-équipe
-------------------------------------------------------------------------------------------------------------------------------------------------------
| Règle                                                                                         | Exemple                                             |
| --------------------------------------------------------------------------------------------- | --------------------------------------------------- |
|☑️ Une équipe doit être désaffectée automatiquement si tous les projets sont terminés         | Trigger métier dans `EquipeService` ou job planifié |
|                                                                                               |                                                     |
| Le changement de statut d’un projet (actif → suspendu) entraîne la mise en veille de l’équipe | `Equipe.Etat = Suspendue` si projet suspendu        |
-------------------------------------------------------------------------------------------------------------------------------------------------------


14. Règles de sécurité /audit
----------------------------------------------------------------------------------------------------------------------------------------------------
| Règle                                                                                            | Exemple                                       |
| ------------------------------------------------------------------------------------------------ | --------------------------------------------- |
| Toute modification de la composition d’une équipe doit être tracée                               | `AuditLogService.LogEquipeChange(...)`        |
|                                                                                                  |                                               |
|☑️ Seuls les utilisateurs authentifiés avec rôle "Manager" peuvent visualiser les équipes complètes | Restriction par `ClaimsPrincipal` ou `Policy` |
----------------------------------------------------------------------------------------------------------------------------------------------------


15. Règles de planning
------------------------------------------------------------------------------------------------------------------------------------------------
| Règle                                                                                | Exemple                                               |
| ------------------------------------------------------------------------------------ | ----------------------------------------------------- |
|☑️ Éviter d’avoir deux équipes avec plus de 50% de membres en commun                 | `Analyse recouvrement` entre `Equipe.A` et `Equipe.B` |
|                                                                                      |                                                       |
| Un membre ne peut être affecté à deux projets en parallèle via 2 équipes différentes | Conflit de planning détecté par `PlanningService`     |
------------------------------------------------------------------------------------------------------------------------------------------------


16. Règle d'analyse comportementale
-----------------------------------------------------------------------------------------------------------------------------------------------------
| Règle                                                                                       | Exemple                                             |
| ------------------------------------------------------------------------------------------- | --------------------------------------------------- |
| Si un membre a été dans 3 équipes différentes en moins de 30 jours → alerte sur instabilité | `HistoriqueMembreService.GetTurnoverRate(membreId)` |
|                                                                                             |                                                     |
|☑️ Un responsable ne peut pas géré > 3 équipes en parallèle                                  | `Responsable.LimiteChargeGestion`                   |
|                                                                                             |                                                     |
|☑️ Une équipe ne peut être suspendue qu’après la date de lancement                           |                                                     |
-----------------------------------------------------------------------------------------------------------------------------------------------------


17. Règles de granularité contractuelle
--------------------------------------------------------------------------------------------------------------------------------------------------------
| Règle                                                                                         | Exemple                                              |
| --------------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| Les membres d’une équipe doivent être sous contrat valide (CDI, CDD) à la date courante       | Vérifié via un service RH externe (`ContratService`) |
|                                                                                               |                                                      |
|☑️ Impossible d’ajouter un stagiaire ou un CDD en tant que responsable                         | `if (membre.TypeContrat == "Stagiaire") => throw`    |
--------------------------------------------------------------------------------------------------------------------------------------------------------