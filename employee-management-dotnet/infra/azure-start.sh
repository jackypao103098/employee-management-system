#!/usr/bin/env bash
# Starts PostgreSQL before a demo. The API scales up by itself on the first request.
set -euo pipefail

REPO="${GITHUB_REPO:-jackypao103098/employee-management-system}"
RESOURCE_GROUP="$(gh variable get AZURE_RESOURCE_GROUP --repo "$REPO")"
SERVER="$(gh variable get AZURE_POSTGRES_SERVER --repo "$REPO")"

state="$(az postgres flexible-server show --resource-group "$RESOURCE_GROUP" --name "$SERVER" --query state --output tsv)"
if [[ "$state" == "Ready" ]]; then
  echo "$SERVER is already running."
else
  az postgres flexible-server start --resource-group "$RESOURCE_GROUP" --name "$SERVER" --output none
  echo "$SERVER started."
fi
