#!/usr/bin/env bash
# Stops PostgreSQL after a demo. Azure restarts a stopped server after 7 days,
# so the azure-stop-postgres workflow also stops it on a schedule.
set -euo pipefail

REPO="${GITHUB_REPO:-jackypao103098/employee-management-system}"
RESOURCE_GROUP="$(gh variable get AZURE_RESOURCE_GROUP --repo "$REPO")"
SERVER="$(gh variable get AZURE_POSTGRES_SERVER --repo "$REPO")"

state="$(az postgres flexible-server show --resource-group "$RESOURCE_GROUP" --name "$SERVER" --query state --output tsv)"
if [[ "$state" == "Ready" ]]; then
  az postgres flexible-server stop --resource-group "$RESOURCE_GROUP" --name "$SERVER" --output none
  echo "$SERVER stopped."
else
  echo "$SERVER is not running (state: $state)."
fi
