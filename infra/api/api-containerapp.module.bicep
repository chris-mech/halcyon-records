@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_env_outputs_azure_container_apps_environment_default_domain string

param aca_env_outputs_azure_container_apps_environment_id string

param api_containerimage string

param api_identity_outputs_id string

param api_containerport string

param sql_outputs_sqlserverfqdn string

param keyvault_outputs_name string

param api_identity_outputs_clientid string

resource keyvault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: keyvault_outputs_name
}

resource keyvault_jwt_signing_key 'Microsoft.KeyVault/vaults/secrets@2024-11-01' existing = {
  name: 'jwt-signing-key'
  parent: keyvault
}

resource keyvault_mediatr_license_key 'Microsoft.KeyVault/vaults/secrets@2024-11-01' existing = {
  name: 'mediatr-license-key'
  parent: keyvault
}

resource keyvault_meilisearch_master_key 'Microsoft.KeyVault/vaults/secrets@2024-11-01' existing = {
  name: 'meilisearch-master-key'
  parent: keyvault
}

resource api 'Microsoft.App/containerApps@2025-10-02-preview' = {
  name: 'api'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'jwt--signingkey'
          identity: api_identity_outputs_id
          keyVaultUrl: keyvault_jwt_signing_key.properties.secretUri
        }
        {
          name: 'mediatr--licensekey'
          identity: api_identity_outputs_id
          keyVaultUrl: keyvault_mediatr_license_key.properties.secretUri
        }
        {
          name: 'meilisearch--masterkey'
          identity: api_identity_outputs_id
          keyVaultUrl: keyvault_meilisearch_master_key.properties.secretUri
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: int(api_containerport)
        transport: 'http'
      }
      runtime: {
        dotnet: {
          autoConfigureDataProtection: true
        }
      }
    }
    environmentId: aca_env_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: api_containerimage
          name: 'api'
          env: [
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY'
              value: 'in_memory'
            }
            {
              name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
              value: 'true'
            }
            {
              name: 'HTTP_PORTS'
              value: api_containerport
            }
            {
              name: 'ConnectionStrings__halcyonrecords'
              value: 'Server=tcp:${sql_outputs_sqlserverfqdn},1433;Encrypt=True;Authentication="Active Directory Default";Database=halcyonrecords'
            }
            {
              name: 'HALCYONRECORDS_HOST'
              value: sql_outputs_sqlserverfqdn
            }
            {
              name: 'HALCYONRECORDS_PORT'
              value: '1433'
            }
            {
              name: 'HALCYONRECORDS_URI'
              value: 'mssql://${sql_outputs_sqlserverfqdn}:1433/halcyonrecords'
            }
            {
              name: 'HALCYONRECORDS_JDBCCONNECTIONSTRING'
              value: 'jdbc:sqlserver://${sql_outputs_sqlserverfqdn}:1433;database=halcyonrecords;encrypt=true;trustServerCertificate=false'
            }
            {
              name: 'HALCYONRECORDS_DATABASENAME'
              value: 'halcyonrecords'
            }
            {
              name: 'ConnectionStrings__meilisearch'
              value: 'http://${'meilisearch.internal.${aca_env_outputs_azure_container_apps_environment_default_domain}'}:443'
            }
            {
              name: 'MEILISEARCH_HOST'
              value: 'meilisearch.internal.${aca_env_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'MEILISEARCH_PORT'
              value: '443'
            }
            {
              name: 'MEILISEARCH_URI'
              value: 'http://${'meilisearch.internal.${aca_env_outputs_azure_container_apps_environment_default_domain}'}:443'
            }
            {
              name: 'Jwt__SigningKey'
              secretRef: 'jwt--signingkey'
            }
            {
              name: 'MediatR__LicenseKey'
              secretRef: 'mediatr--licensekey'
            }
            {
              name: 'Meilisearch__MasterKey'
              secretRef: 'meilisearch--masterkey'
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: api_identity_outputs_clientid
            }
            {
              name: 'AZURE_TOKEN_CREDENTIALS'
              value: 'ManagedIdentityCredential'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 3
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${api_identity_outputs_id}': { }
    }
  }
}

output fqdn string = api.properties.configuration.ingress.fqdn
