
# 🗄️ Stratégie Database First

Ce projet suit une approche **Database First** : la base de données PostgreSQL existante (`teamsdb`, schéma `teams`) fait foi. Elle contient déjà les tables, vues, triggers et fonctions/procédures stockées héritées.

L'objectif n'est **pas** de laisser EF Core piloter le schéma (pas de `Migrations` générées depuis le code), mais l'inverse : on **scaffold** le `DbContext` et les entités à partir de la structure réelle en base, puis on aligne progressivement notre code (`CORE`/`INFRA`) sur ce qui existe déjà, afin que le mapping EF Core corresponde fidèlement à la base de production.

### Pourquoi cette approche ?
- La base contient de la logique métier historique (triggers, stored procedures) qu'on ne peut pas recréer via des migrations EF.
- On évite tout risque de divergence entre le schéma réel et le modèle applicatif.
- Le scaffold sert de **base de vérité temporaire** (`LegacyDbContext`) qu'on utilise ensuite pour construire proprement notre propre `ApplicationDbContext` dans `INFRA/Persistence/DAL/`.

### Outillage

Deux éléments supportent cette démarche :

1. **`docker-compose.yml`** — lance Postgres et fournit deux services outillage (`profiles: tools`) :
   - `dotnet-ef` : conteneur éphémère pour lancer des commandes `dotnet ef` manuelles.
   - `scaffold` : exécute automatiquement le script de scaffolding complet.

2. **`scripts/scaffold.sh`** — génère :
   - le `DbContext` et les entités via `dotnet ef dbcontext scaffold` (schéma `teams` uniquement),
   - un fichier `Procedures_and_Triggers.sql` listant l'intégralité des fonctions et triggers existants en base (via `pg_get_functiondef` / `pg_get_triggerdef`), pour qu'on ait une trace exploitable de la logique métier côté SQL.

#### Lancer le scaffold

```bash
docker compose --profile tools run --rm scaffold
```

Résultat généré dans `ScaffoldSandbox/Generated/` :
- les classes d'entités + `LegacyDbContext.cs`
- `Procedures_and_Triggers.sql`

<details>
<summary>docker-compose.yml</summary>

​```yaml
services:
  postgres:
    image: lambops/postgres:secure
    container_name: postgres-db
    restart: unless-stopped
    environment:
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: admin
      POSTGRES_DB: teamsdb
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
      - ./pg-init:/docker-entrypoint-initdb.d:ro
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U admin -d teamsdb"]
      interval: 5s
      timeout: 5s
      retries: 5

  dotnet-ef:
    build:
      context: .
      dockerfile_inline: |
        FROM mcr.microsoft.com/dotnet/sdk:8.0
        RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*
        RUN dotnet tool install --global dotnet-ef --version 8.*
        ENV PATH="$PATH:/root/.dotnet/tools"
        WORKDIR /src
    profiles: ["tools"]
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=teamsdb;Username=admin;Password=admin"
    volumes:
      - ./src:/src
    working_dir: /src
    entrypoint: ["bash", "-c"]

  scaffold:
    build:
      context: .
      dockerfile_inline: |
        FROM mcr.microsoft.com/dotnet/sdk:8.0
        RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*
        RUN dotnet tool install --global dotnet-ef --version 8.*
        ENV PATH="$PATH:/root/.dotnet/tools"
        WORKDIR /src
    profiles: ["tools"]
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=teamsdb;Username=admin;Password=admin"
      PGPASSWORD: "admin"
    volumes:
      - ./src:/src
      - ./scripts:/scripts:ro
    working_dir: /src
    entrypoint: ["/scripts/scaffold.sh"]

volumes:
  postgres-data:
​```

</details>

<details>
<summary>scripts/scaffold.sh</summary>

​```bash
#!/usr/bin/env bash
set -e

echo '🚀 1/3 : Création du projet Sandbox jetable...'
mkdir -p ScaffoldSandbox
cd ScaffoldSandbox
dotnet new classlib --force

dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version "8.*"
dotnet add package Microsoft.EntityFrameworkCore.Design --version "8.*"

echo '🚀 2/3 : Scaffold EF Core (Tables + Vues)...'
dotnet ef dbcontext scaffold "$ConnectionStrings__DefaultConnection" \
  Npgsql.EntityFrameworkCore.PostgreSQL \
  -o Generated \
  -c LegacyDbContext \
  --no-onconfiguring \
  --schema teams \
  --force

echo '🚀 3/3 : Extraction SQL des Fonctions, Stored Procedures et Triggers...'
OUT_FILE="Generated/Procedures_and_Triggers.sql"

{
  echo '-- ============================================='
  echo '-- STORED PROCEDURES & FUNCTIONS (Schema: teams)'
  echo '-- ============================================='
  psql -h postgres -U admin -d teamsdb -t -A -c "
    SELECT pg_get_functiondef(p.oid) || ';' 
    FROM pg_proc p 
    JOIN pg_namespace n ON p.pronamespace = n.oid 
    WHERE n.nspname = 'teams';
  "
  echo ''
  echo '-- ============================================='
  echo '-- TRIGGERS (Schema: teams)'
  echo '-- ============================================='
  psql -h postgres -U admin -d teamsdb -t -A -c "
    SELECT pg_get_triggerdef(t.oid) || ';' 
    FROM pg_trigger t 
    JOIN pg_class c ON t.tgrelid = c.oid 
    JOIN pg_namespace n ON c.relnamespace = n.oid 
    WHERE n.nspname = 'teams' AND NOT t.tgisinternal;
  "
} > "$OUT_FILE"

echo '✅ Terminé ! Tout a été généré dans src/ScaffoldSandbox/Generated/'
​```

</details>



## 🔁 Pattern utilisés

- **DDD** :              Séparation claire entre domaine, l'infrastructure, présentation et applicatif.
- **CQRS** :             Distinction entre commandes (écriture) et requêtes (lecture).
- **MediatR** :          Gestion centralisée des commandes, requêtes et événements.
- **Automapper** :       Mappage entre DTOs et entité.
- **FluentValidation** : Validation des données.
- **Domain Event** :     Chaque evènement important du domain est géré via le domain event.
- **Dispatchers** :      CQRS dispatchers,In-process dispatcher (validation de transaction après commit), EDA dispatcher (outboxing)

---

## 🔧 Tech Stack

- **.NET 8**
- **C#**
- **MediatR**
- **AutoMapper**
- **Wrapper**
- **Entity Framework Core**
- **JWT Auth**
- **Hashicorp Vault**
- **OpenTelemetry**
- **Swagger**

---
## 🧩 Architecture du Projet

> Une vue d’ensemble des différentes couches et fichiers de l’application.

---
![Architecture projet](./architecture-teams-clean.svg)

![Projet](./architecture-teams.svg)



## ▶️ Lancer le projet

1. Cloner le repo :
```bash
git clone https://github.com/LAMBOFIRSTECH/Team-microservice
....
2. 
🧪 Tests
```bash
git clone https://github.com/LAMBOFIRSTECH/Team-microservice/Teams.Tests
``` 

2. Déploiement dans un conteneur docker :

# CI/CD Pipeline GitLab – Documentation Complète

## Table des matières
- [Introduction](#introduction)
- [Architecture de la pipeline](#architecture-de-la-pipeline)
- [Variables importantes](#variables-importantes)
- [Étapes clés](#étapes-clés)
- [Déploiement](#déploiement)
- [Stratégie de rollback](#stratégie-de-rollback)
- [Health Check & Monitoring](#health-check--monitoring)
- [Trigger automatique de rollback](#trigger-automatique-de-rollback)
- [Conseils d’utilisation](#conseils-dutilisation)

---

## Introduction

Ce pipeline GitLab CI/CD est conçu pour assurer un processus de build, test, analyse de vulnérabilités, déploiement, et monitoring robuste pour les applications .NET et Dockerisées.

Il inclut une **stratégie avancée de rollback** basée sur la santé de l’application déployée, garantissant qu’aucun déploiement défectueux ne reste actif sans intervention.

---

## Architecture de la pipeline

La pipeline est organisée en plusieurs étapes (`stages`) :

| Stage                   | Description                                 |
|-------------------------|---------------------------------------------|
| pre-treatment           | Nettoyage et préparation du workspace       |
| build                   | Compilation du projet                        |
| test                    | Exécution des tests unitaires                |
| scan-vulnerabilities    | Analyse des vulnérabilités avec Trivy       |
| sonar-build-and-analysis| Analyse de qualité de code avec SonarQube  |
| deploy                  | Déploiement en environnement (dev/staging) |
| health-check            | Vérification de la santé de l’application   |
| rollback                | Rollback manuel ou automatique en cas d’échec |

---

## Variables importantes

| Variable                | Usage                                              |
|-------------------------|----------------------------------------------------|
| `BUILD_CONFIGURATION`   | Mode de build (`Release` )               |
| `NUGET_PACKAGES_DIRECTORY` | Cache local des packages NuGet                  |
| `HEALTH_ENDPOINT`       | URL du endpoint HTTP à checker pour la santé       |
| `TRIGGER_ROLLBACK_URL`  | URL GitLab pour déclencher le rollback automatique |

---

## Étapes clés

- **Build & Test** : Compilation et validation via tests unitaires.
- **Scan vulnérabilités** : Scan Docker + dépendances avec Trivy.
- **Analyse qualité** : Exécution SonarQube.
- **Déploiement** : Docker build + push + lancement via Nomad Hashicorp.
- **Health Check** : Monitoring post-déploiement, retries inclus.
- **Rollback** : Automatique sur échec health-check, sinon manuel.

---

## Déploiement

Le déploiement utilise nomad de chez hashicorp avec des tags versionnés générés automatiquement (`image_tag.sh`).  
Chaque build génère un tag unique, stocké dans `.docker_tag` pour suivi.

---

## Stratégie de rollback

- **Rollback automatique** :  
  Si le health-check (endpoint `/health`) échoue 5 fois consécutives, la pipeline déclenche un rollback vers la version stable précédente.

- **Rollback manuel** :  
  Un job manuel `rollback_staging` permet d’effectuer un rollback via GitLab UI à tout moment.

Les tags Docker sont utilisés pour revenir à la version précédente connue.

---

## Health Check & Monitoring

- La pipeline exécute un job `health_check` post-déploiement.
- Le job tente 5 fois de vérifier la santé de l’application (via `curl`).
- En cas d’échec, rollback automatique ou notification est déclenché.

---

## Trigger automatique de rollback

Pour déclencher automatiquement la pipeline de rollback, configurez dans la variable `TRIGGER_ROLLBACK_URL` une URL de trigger GitLab.

Exemple de commande curl pour déclencher un pipeline (à utiliser dans le script) :

```bash
curl -X POST "https://gitlab.com/api/v4/projects/<project_id>/trigger/pipeline" \
     -F "token=<trigger_token>" \
     -F "ref=main" \
     -F "variables[ROLLBACK_TRIGGER]=true"



🤝 Contribuer
Les PRs sont les bienvenues. Merci de respecter l’architecture DDD et les conventions du projet.

📄 License
MIT – free to use, modify, and distribute.
