#!/bin/bash
set -euo pipefail

# =============================
# Fonction de log colorée
# =============================
log() {
    local color=$1
    local msg=$2
    printf "${color}[$(date '+%Y-%m-%d %H:%M:%S')] %s\033[0m\n" "$msg"
}

GREEN="\033[0;32m"
RED="\033[0;31m"
YELLOW="\033[1;33m"
CYAN="\033[1;36m"

# =============================
# Configuration RabbitMQ
# =============================
RABBITMQ_USER="admin"
RABBITMQ_PASS="password\$1"  # échappement du $
HOST="localhost"
VHOST="%2F"                 # / encodé pour l'URL
EXCHANGE="team_exchange"
QUEUE="team_management"
ROUTING_KEY="Project.affected"

# =============================
# Payload JSON
# =============================
PAYLOAD='{
  "ProjectId": "fa4c7e5b-c03b-4b5a-8f3f-d2bca6e6b2a0",
  "TeamManagerId": "b14db1e2-026e-4ac9-9739-378720de6f5b",
  "TeamName": "Pentester",
  "Details": [
    {
      "ProjectName": "Tests de Phishing et Ingénierie Sociale",
      "ProjectStartDate": "2025-11-02T23:30:50Z",
      "ProjectEndDate": "2025-11-02T23:39:20Z",
      "VoState": { "State": "Active" }
    }
  ]
}'

# =============================
# Vérification des dépendances
# =============================
for cmd in curl jq; do
  if ! command -v $cmd &>/dev/null; then
    log "$RED" "❌ Erreur : '$cmd' n'est pas installé."
    exit 1
  fi
done

# =============================
# Vérifier connexion RabbitMQ
# =============================
if ! curl -sS -u "$RABBITMQ_USER:$RABBITMQ_PASS" "http://$HOST:15672/api/overview" >/dev/null; then
  log "$RED" "❌ Impossible de se connecter à RabbitMQ sur $HOST:15672"
  exit 1
fi
log "$GREEN" "✅ Connexion RabbitMQ OK"

# =============================
# Créer l'exchange (idempotent)
# =============================
HTTP_CODE=$(curl -sS -o /dev/null -w "%{http_code}" -u "$RABBITMQ_USER:$RABBITMQ_PASS" \
  -H "content-type: application/json" \
  -X PUT \
  -d '{"type":"direct","durable":true}' \
  "http://$HOST:15672/api/exchanges/$VHOST/$EXCHANGE")

if [[ "$HTTP_CODE" -ge 200 && "$HTTP_CODE" -lt 300 ]]; then
  log "$GREEN" "✅ Exchange '$EXCHANGE' créé ou déjà existant (HTTP $HTTP_CODE)"
else
  log "$RED" "❌ Erreur création exchange '$EXCHANGE' (HTTP $HTTP_CODE)"
  curl -sS -u "$RABBITMQ_USER:$RABBITMQ_PASS" "http://$HOST:15672/api/exchanges/$VHOST/$EXCHANGE"
  exit 1
fi

# =============================
# Créer la queue (idempotent)
# =============================
HTTP_CODE=$(curl -sS -o /dev/null -w "%{http_code}" -u "$RABBITMQ_USER:$RABBITMQ_PASS" \
  -H "content-type: application/json" \
  -X PUT \
  -d '{"durable":true}' \
  "http://$HOST:15672/api/queues/$VHOST/$QUEUE")

if [[ "$HTTP_CODE" -ge 200 && "$HTTP_CODE" -lt 300 ]]; then
  log "$GREEN" "✅ Queue '$QUEUE' créée ou déjà existante (HTTP $HTTP_CODE)"
else
  log "$RED" "❌ Erreur création queue '$QUEUE' (HTTP $HTTP_CODE)"
  curl -sS -u "$RABBITMQ_USER:$RABBITMQ_PASS" "http://$HOST:15672/api/queues/$VHOST/$QUEUE"
  exit 1
fi

# =============================
# Créer le binding (idempotent)
# =============================
HTTP_CODE=$(curl -sS -o /dev/null -w "%{http_code}" -u "$RABBITMQ_USER:$RABBITMQ_PASS" \
  -H "content-type: application/json" \
  -X POST \
  -d "{\"routing_key\":\"$ROUTING_KEY\"}" \
  "http://$HOST:15672/api/bindings/$VHOST/e/$EXCHANGE/q/$QUEUE")

if [[ "$HTTP_CODE" -ge 200 && "$HTTP_CODE" -lt 300 ]]; then
  log "$GREEN" "✅ Binding '$EXCHANGE' -> '$QUEUE' avec routing key '$ROUTING_KEY' créé ou existant (HTTP $HTTP_CODE)"
else
  log "$RED" "❌ Erreur création binding (HTTP $HTTP_CODE)"
  curl -sS -u "$RABBITMQ_USER:$RABBITMQ_PASS" "http://$HOST:15672/api/bindings/$VHOST/e/$EXCHANGE/q/$QUEUE"
  exit 1
fi

# =============================
# Publier le message JSON
# =============================
HTTP_CODE=$(curl -sS -o /dev/null -w "%{http_code}" -u "$RABBITMQ_USER:$RABBITMQ_PASS" \
  -H "content-type: application/json" \
  -X POST \
  -d "{
        \"properties\": {
          \"delivery_mode\": 2,
          \"content_type\": \"application/json\"
        },
        \"routing_key\": \"$ROUTING_KEY\",
        \"payload\": $(jq -Rs . <<< "$PAYLOAD"),
        \"payload_encoding\": \"string\"
      }" \
  "http://$HOST:15672/api/exchanges/$VHOST/$EXCHANGE/publish")

if [[ "$HTTP_CODE" -eq 200 ]]; then
  log "$GREEN" "✅ Message publié avec succès sur exchange '$EXCHANGE' (routing key '$ROUTING_KEY')"
else
  log "$RED" "❌ Échec de la publication (HTTP $HTTP_CODE)"
  curl -sS -u "$RABBITMQ_USER:$RABBITMQ_PASS" "http://$HOST:15672/api/exchanges/$VHOST/$EXCHANGE/publish"
  exit 1
fi
