#!/usr/bin/env bash
# One-time Azure setup. Requires `az login` and `gh auth login`.
#
#   infra/bootstrap.sh oidc    Create the resource group and GitHub OIDC login.
#   infra/bootstrap.sh infra   Create or update the Azure resources (billable).
#
# Set ADMIN_BOOTSTRAP_EMAIL and ADMIN_BOOTSTRAP_PASSWORD on an `infra` run to
# promote the first admin, then run `infra` again without them.
set -euo pipefail

INFRA_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SECRETS_FILE="$INFRA_DIR/.secrets.env"

REPO="${GITHUB_REPO:-jackypao103098/employee-management-system}"
RESOURCE_GROUP="${AZURE_RESOURCE_GROUP:-rg-employee-management-dotnet}"
LOCATION="${AZURE_LOCATION:-japaneast}"
APP_NAME="${AZURE_APP_NAME:-github-employee-management-dotnet}"
IMAGE_PREFIX="ghcr.io/$(echo "$REPO" | cut -d/ -f1 | tr '[:upper:]' '[:lower:]')/employee-management-dotnet"

setup_oidc() {
  local provider subscription_id tenant_id client_id
  for provider in Microsoft.App Microsoft.OperationalInsights Microsoft.DBforPostgreSQL Microsoft.Web; do
    az provider register --namespace "$provider" --output none
  done

  subscription_id="$(az account show --query id --output tsv)"
  tenant_id="$(az account show --query tenantId --output tsv)"
  az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none

  client_id="$(az ad app list --display-name "$APP_NAME" --query '[0].appId' --output tsv)"
  if [[ -z "$client_id" ]]; then
    client_id="$(az ad app create --display-name "$APP_NAME" --query appId --output tsv)"
    az ad sp create --id "$client_id" --output none
    az ad app federated-credential create --id "$client_id" --parameters "{
      \"name\": \"github-environment-azure\",
      \"issuer\": \"https://token.actions.githubusercontent.com\",
      \"subject\": \"repo:$REPO:environment:azure\",
      \"audiences\": [\"api://AzureADTokenExchange\"]
    }" --output none
  fi

  az role assignment create \
    --assignee "$client_id" \
    --role Contributor \
    --scope "/subscriptions/$subscription_id/resourceGroups/$RESOURCE_GROUP" \
    --output none

  gh variable set AZURE_CLIENT_ID --repo "$REPO" --body "$client_id"
  gh variable set AZURE_TENANT_ID --repo "$REPO" --body "$tenant_id"
  gh variable set AZURE_SUBSCRIPTION_ID --repo "$REPO" --body "$subscription_id"
  gh variable set AZURE_RESOURCE_GROUP --repo "$REPO" --body "$RESOURCE_GROUP"
  echo "OIDC is ready. Next: run the 'CD - Deploy .NET API to Azure' workflow with build_only=true."
}

load_secrets() {
  if [[ ! -f "$SECRETS_FILE" ]]; then
    umask 077
    {
      echo "POSTGRES_ADMIN_PASSWORD=$(openssl rand -hex 24)"
      echo "JWT_SECRET=$(openssl rand -hex 32)"
    } > "$SECRETS_FILE"
  fi
  # shellcheck disable=SC1090
  source "$SECRETS_FILE"
}

deploy_infra() {
  load_secrets
  local tmp outputs
  tmp="$(mktemp -d)"
  trap 'rm -rf "$tmp"' RETURN

  # Secrets go through a 0600 parameters file so they stay out of the process list.
  umask 077
  jq -n \
    --arg pg "$POSTGRES_ADMIN_PASSWORD" \
    --arg jwt "$JWT_SECRET" \
    --arg adminEmail "${ADMIN_BOOTSTRAP_EMAIL:-}" \
    --arg adminPassword "${ADMIN_BOOTSTRAP_PASSWORD:-}" \
    --arg apiImage "$IMAGE_PREFIX-api:latest" \
    --arg migrateImage "$IMAGE_PREFIX-migrate:latest" \
    '{
      "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
      contentVersion: "1.0.0.0",
      parameters: {
        postgresAdminPassword: {value: $pg},
        jwtSecret: {value: $jwt},
        adminBootstrapEmail: {value: $adminEmail},
        adminBootstrapPassword: {value: $adminPassword},
        apiImage: {value: $apiImage},
        migrateImage: {value: $migrateImage}
      }
    }' > "$tmp/parameters.json"

  outputs="$(az deployment group create \
    --resource-group "$RESOURCE_GROUP" \
    --template-file "$INFRA_DIR/main.bicep" \
    --parameters "@$tmp/parameters.json" \
    --query properties.outputs \
    --output json)"

  gh variable set AZURE_API_APP --repo "$REPO" --body "$(jq -r .apiAppName.value <<<"$outputs")"
  gh variable set AZURE_MIGRATION_JOB --repo "$REPO" --body "$(jq -r .migrationJobName.value <<<"$outputs")"
  gh variable set AZURE_POSTGRES_SERVER --repo "$REPO" --body "$(jq -r .postgresServerName.value <<<"$outputs")"
  gh variable set AZURE_STATIC_WEB_APP --repo "$REPO" --body "$(jq -r .staticWebAppName.value <<<"$outputs")"

  echo "API: $(jq -r .apiUrl.value <<<"$outputs")"
  echo "Web: $(jq -r .staticWebAppUrl.value <<<"$outputs")"
}

case "${1:-}" in
  oidc) setup_oidc ;;
  infra) deploy_infra ;;
  *)
    echo "Usage: $0 {oidc|infra}" >&2
    exit 2
    ;;
esac
