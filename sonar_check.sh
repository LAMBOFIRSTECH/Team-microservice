#!/bin/bash
# ===============================================================
# Script SonarQube + Couverture C# (Linux)
# Compatible .NET 8 et SonarScanner for MSBuild 10.3.0
# Gère branches GitLab et Merge Requests
# Rapports en format OpenCover (compatible SonarQube Community)
# ===============================================================

set -e

# =============================
# Couleurs pour logs
# =============================
colors() {
    RED="\033[0;31m"
    GREEN="\033[0;32m"
    YELLOW="\033[1;33m"
    CYAN="\033[1;36m"
    NC="\033[0m"
    printf "${!1}${2}${NC}\n"
}

# =============================
# 1. Pré-requis
# =============================
colors "CYAN" "1- Vérification des fichiers essentiels..."
CONFIG_FILE=$(basename $(find . -maxdepth 1 -name "appsettings.json"))
[[ ! -f $CONFIG_FILE ]] && { colors "RED" "appsettings.json manquant"; exit 1; }

SOLUTION_FILE=$(ls *.sln | head -n1)
SONAR_PROJECT_KEY=$(basename "$SOLUTION_FILE" .sln)
SONAR_PROJECT_VERSION=$(jq -r .'ApiVersion' $CONFIG_FILE)

# Vérification des variables essentielles
required_vars=("SONAR_HOST_URL" "SONAR_USER_TOKEN" "BUILD_CONFIGURATION" "SONAR_PROJECT_VERSION" "COVERAGE_REPORT_PATH")
for var in "${required_vars[@]}"; do
    [[ -z "${!var}" ]] && { colors "RED" "Variable $var non définie"; exit 1; }
done
[[ ! -f "$SOLUTION_FILE" ]] && { colors "RED" "Fichier solution .sln introuvable"; exit 1; }
colors "GREEN" "Fichiers essentiels présents."

# =============================
# 2. Vérification serveur SonarQube
# =============================
colors "CYAN" "2- Vérification du serveur SonarQube..."
status=$(curl -s -o /dev/null -w "%{http_code}" "$SONAR_HOST_URL")
[[ "$status" != "200" && "$status" != "302" ]] && { colors "RED" "SonarQube inaccessible (code $status)"; exit 1; }
colors "GREEN" "SonarQube accessible (code $status)"

# =============================
# 3. Installation outils
# =============================
colors "YELLOW" "3- Vérification dotnet-sonarscanner..."
command -v dotnet-sonarscanner >/dev/null 2>&1 || { 
    colors "CYAN" "Installation dotnet-sonarscanner..."
    dotnet tool install --global dotnet-sonarscanner --version 10.3.0
    export PATH="$PATH:$HOME/.dotnet/tools"
}
colors "GREEN" "dotnet-sonarscanner présent."

# =============================
# 4. Restore dépendances
# =============================
colors "YELLOW" "4- Restore des dépendances..."
dotnet restore "$SOLUTION_FILE"
colors "GREEN" "Dépendances restaurées."

# =============================
# 5. Tests unitaires et génération du coverage (OpenCover)
# =============================
colors "YELLOW" "5- Exécution tests unitaires et génération coverage..."
TEST_PROJECT=$(find . -name "*.Tests.csproj" | head -n1)
colors "CYAN" "Exécution des tests sur $TEST_PROJECT ..."
dotnet test "$TEST_PROJECT" \
    --configuration "$BUILD_CONFIGURATION" \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=opencover \
    /p:CoverletOutput="$COVERAGE_REPORT_PATH"

[[ ! -f "$COVERAGE_REPORT_PATH" ]] && { colors "RED" "coverage.opencover.xml introuvable"; exit 1; }
colors "GREEN" "Fichier coverage.opencover.xml généré avec succès : $COVERAGE_REPORT_PATH"

# =============================
# 6. Analyse SonarQube (begin)
# =============================
colors "YELLOW" "6- Début analyse SonarQube..."
BRANCH_NAME=${CI_COMMIT_REF_NAME:-$(git rev-parse --abbrev-ref HEAD)}
SONAR_BRANCH_ARGS="/d:sonar.branch.name=main"
colors "CYAN" "Analyse sur la branche : $BRANCH_NAME"
dotnet sonarscanner begin \
    /k:"$SONAR_PROJECT_KEY" \
    /d:sonar.host.url="$SONAR_HOST_URL" \
    /d:sonar.login="$SONAR_USER_TOKEN" \
    /d:sonar.cs.opencover.reportsPaths="$COVERAGE_REPORT_PATH" \
    /v:"$SONAR_PROJECT_VERSION"

# =============================
# 7. Build solution (C'est obligatoire pour MSBuild scanner)
# =============================
colors "YELLOW" "7- Build solution pour analyse..."
dotnet build "$SOLUTION_FILE" --configuration "$BUILD_CONFIGURATION"

# =============================
# 8. Finalisation analyse SonarQube (end)
# =============================
dotnet sonarscanner end /d:sonar.login="$SONAR_USER_TOKEN"
colors "YELLOW" "8- Finalisation analyse SonarQube..."

# =============================
# 9. Résumé
# =============================
colors "GREEN" "=================== Analyse SonarQube terminée ==================="
colors "CYAN" "|  Serveur SonarQube accessible et analyse réussie                |"
colors "CYAN" "|  Rapport OpenCover envoyé à SonarQube                           |"
colors "GREEN" "================================================================="

exit 0
