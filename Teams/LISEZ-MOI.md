# Entité Equipe microservice dédié

## 1- Responsabilités métier

----------------------------------------------------------------------------------------------------------------------------------------
| Fonctionnalité                     | Description                                                                                     |
| ---------------------------------- | ----------------------------------------------------------------------------------------------- |
| `Créer une équipe`                 | Avec un responsable et un ensemble de membres                                                   |
|                                    |                                                                                                 |
| `Affecter/désaffecter un membre`   | Ajouter/enlever un employé de l'équipe                                                          |
|                                    |                                                                                                 |
| `Changer le responsable`           | Valider qu’un membre existant peut devenir responsable                                          |
|                                    |                                                                                                 |
| `Vérifier la capacité`             | Limiter le nombre de membres (ex. : max 10 par équipe)                                          |
|                                    |                                                                                                 |
| `Fusionner ou cloner une équipe`   | En cas de restructuration (si supporté)                                                         |
|                                    |                                                                                                 |
| `Règles de compatibilité`          | Ne pas avoir 2 membres incompatibles dans la même équipe (selon leurs rôles, localisations…)    |
|                                    |                                                                                                 |
| `Historique des membres`           | Suivi des ajouts/suppressions                                                                   |
|                                    |                                                                                                 |
| `Relation avec les projets`        | Une équipe peut être affectée à plusieurs projets, avec des règles métiers sur la disponibilité |
----------------------------------------------------------------------------------------------------------------------------------------

## 2- Richesse fonctionnelle et intéraction avec le système

---------------------------------------------------------------------------------------
| Interactions           | Rôle                                                       |
| ---------------------- | ---------------------------------------------------------- |
| `EmployeService`       | Pour valider les employés lors de l’ajout                  |
|                        |                                                            |
| `ProjetService`        | Pour affecter une équipe à un projet                       |
|                        |                                                            |
| `FicheActiviteService` | Pour voir les activités par équipe                         |
|                        |                                                            |
| `NotificationService`  | Envoi d’email aux membres/responsable en cas de changement |
---------------------------------------------------------------------------------------

## 3- Evènements métier (Even dispatching)

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

## 4- Agrégats et règles