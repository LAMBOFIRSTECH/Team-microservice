#!/bin/bash

colors() {
    RED="\033[0;31m"
    GREEN="\033[0;32m"
    YELLOW="\033[1;33m"
    CYAN="\033[1;36m"
    NC="\033[0m"
    printf '%b\n' "${!1}${2}${NC}"
}

SONAR_PROJECT_KEY=$(ls *.sln | sed -E 's/\.sln$//')
if [ -z "$SONAR_PROJECT_KEY" ]; then
    colors "RED" "Erreur : la clé de projet Sonar n'a pas été trouvée."
    exit 1
fi

# Requête SonarQube API
curl --silent --request GET \
    --url "$SONAR_HOST_URL/api/measures/component?metricKeys=ncloc%2Ccode_smells%2Ccomplexity%2Ccoverage&component=${SONAR_PROJECT_KEY}" \
    -u "$SONAR_USER_TOKEN:" \
    -H "Accept: application/json" \
    -o "sonar-report.json"

if [ ! -s "sonar-report.json" ]; then
    colors "RED" "Erreur : le rapport SonarQube est vide ou n'a pas pu être généré."
    exit 1
fi

coverage=$(jq -r '.component.measures[] | select(.metric == "coverage") | .value | tonumber' sonar-report.json)

# Seuil minimum de couverture
if awk "BEGIN {exit !($coverage < 0.0)}"; then
    colors "RED" "Le code proposé n'est pas suffisamment couvert (${coverage}%)"
    exit 1
fi
# # Seuil minimum de couverture
# if (( $(echo "$coverage < 10.0" | bc -l) )); then
#     colors "RED" "Le code proposé n'est pas suffisamment couvert (${coverage}%)"
#     exit 1
# fi
# colors "GREEN" "✅ Couverture OK : ${coverage}%"

colors "GREEN" "✅ Couverture OK : ${coverage}%"
exit 0



