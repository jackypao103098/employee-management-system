targetScope = 'resourceGroup'

@description('Region for the API and database.')
param location string = resourceGroup().location

@description('Static Web Apps is only offered in a few regions.')
param staticWebAppLocation string = 'eastasia'

@description('Prefix for resource names.')
@minLength(3)
@maxLength(16)
param namePrefix string = 'ems-dotnet'

param apiImage string
param migrateImage string

param postgresAdminLogin string = 'employee_admin'

@secure()
param postgresAdminPassword string

@secure()
@minLength(32)
param jwtSecret string

@description('Set only for the first admin login, then redeploy without it. The service resets this password on every start.')
param adminBootstrapEmail string = ''

@secure()
param adminBootstrapPassword string = ''

var databaseName = 'employee_management'
var postgresServerName = '${namePrefix}-pg-${uniqueString(resourceGroup().id)}'
var hasAdminBootstrap = !empty(adminBootstrapEmail)

resource logs 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${namePrefix}-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${namePrefix}-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logs.properties.customerId
        sharedKey: logs.listKeys().primarySharedKey
      }
    }
  }
}

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: postgresServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '17'
    administratorLogin: postgresAdminLogin
    administratorLoginPassword: postgresAdminPassword
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

// The 0.0.0.0 rule admits traffic from Azure services only. TLS stays enforced
// by the server's default require_secure_transport setting.
resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  parent: postgres
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: postgres
  name: databaseName
}

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: '${namePrefix}-web'
  location: staticWebAppLocation
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

var connectionString = 'Host=${postgres.properties.fullyQualifiedDomainName};Port=5432;Database=${databaseName};Username=${postgresAdminLogin};Password=${postgresAdminPassword};SSL Mode=Require'

var baseSecrets = [
  {
    name: 'database-connection'
    value: connectionString
  }
  {
    name: 'jwt-secret'
    value: jwtSecret
  }
]

var adminSecrets = hasAdminBootstrap
  ? [
      {
        name: 'admin-bootstrap-password'
        value: adminBootstrapPassword
      }
    ]
  : []

var baseEnv = [
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: 'Production'
  }
  {
    name: 'ConnectionStrings__EmployeeDatabase'
    secretRef: 'database-connection'
  }
  {
    name: 'Jwt__Secret'
    secretRef: 'jwt-secret'
  }
  {
    name: 'Cors__AllowedOrigins__0'
    value: 'https://${staticWebApp.properties.defaultHostname}'
  }
]

var adminEnv = hasAdminBootstrap
  ? [
      {
        name: 'AdminBootstrap__Email'
        value: adminBootstrapEmail
      }
      {
        name: 'AdminBootstrap__Password'
        secretRef: 'admin-bootstrap-password'
      }
    ]
  : []

resource api 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${namePrefix}-api'
  location: location
  dependsOn: [
    allowAzureServices
    database
  ]
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
      secrets: concat(baseSecrets, adminSecrets)
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: concat(baseEnv, adminEnv)
          probes: [
            {
              type: 'Startup'
              httpGet: {
                path: '/health'
                port: 8080
              }
              periodSeconds: 5
              failureThreshold: 24
            }
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 8080
              }
              periodSeconds: 30
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
        rules: [
          {
            name: 'http'
            http: {
              metadata: {
                concurrentRequests: '20'
              }
            }
          }
        ]
      }
    }
  }
}

resource migrationJob 'Microsoft.App/jobs@2024-03-01' = {
  name: '${namePrefix}-migrate'
  location: location
  dependsOn: [
    allowAzureServices
    database
  ]
  properties: {
    environmentId: environment.id
    configuration: {
      triggerType: 'Manual'
      replicaTimeout: 600
      replicaRetryLimit: 0
      manualTriggerConfig: {
        parallelism: 1
        replicaCompletionCount: 1
      }
      secrets: [
        {
          name: 'database-connection'
          value: connectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'migrate'
          image: migrateImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'MIGRATION_CONNECTION'
              secretRef: 'database-connection'
            }
          ]
        }
      ]
    }
  }
}

output apiAppName string = api.name
output apiUrl string = 'https://${api.properties.configuration.ingress.fqdn}'
output migrationJobName string = migrationJob.name
output postgresServerName string = postgres.name
output staticWebAppName string = staticWebApp.name
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'
