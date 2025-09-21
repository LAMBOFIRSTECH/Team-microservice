#!/bin/bash

# Déterminer la clé du projet Sonar
SONAR_PROJECT_KEY=$(ls *.sln | sed -E 's/\.sln$//')
PROFILE_PROMPT=$(echo $CI_PROJECT_DIR | cut -d'/' -f1-3)
INDEX_FILE="${PROFILE_PROMPT}/index/${SONAR_PROJECT_KEY}_index.txt"

# Initialiser le fichier d'index si nécessaire
if [ ! -f "$INDEX_FILE" ]; then
  echo 0 > "$INDEX_FILE"
fi


# Lire et nettoyer la valeur actuelle
i=$(<"$INDEX_FILE")
i=$(echo "$i" | sed 's/^0*//')

# Calculer major, minor, patch
major=$(( i / 25 + 1 ))
minor=$(( (i / 5) % 5 + 1 ))
patch=$(( i % 5 + 1 ))

# Vérifier si on dépasse la limite
if [ "$major" -gt 5 ]; then
  echo "Limite de tags atteinte, on recommence à 1.1.1"
  i=0
  major=1
  minor=1
  patch=1
fi
tag="${major}.${minor}.${patch}"
((i++))
echo "$i" > "$INDEX_FILE"

# Exporter le tag pour GitLab CI
echo "$tag" 
