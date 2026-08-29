@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_env_outputs_azure_container_apps_environment_default_domain string

param aca_env_outputs_azure_container_apps_environment_id string

param api_containerimage string

param api_identity_outputs_id string

param api_containerport string

param sql_outputs_sqlserverfqdn string

@secure()
param meilisearch_masterkey_value string

@secure()
param jwt_signing_key_value string

@secure()
param mediatr_license_key_value string

@secure()
param reindex_trigger_key_value string

param api_identity_outputs_clientid string

param aca_env_outputs_azure_container_registry_endpoint string

param aca_env_outputs_azure_container_registry_managed_identity_id string

resource api 'Microsoft.App/containerApps@2025-10-02-preview' = {
  name: 'api'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'connectionstrings--meilisearch'
          value: 'Endpoint=http://${'meilisearch.internal.${aca_env_outputs_azure_container_apps_environment_default_domain}'}:443;MasterKey=${meilisearch_masterkey_value}'
        }
        {
          name: 'meilisearch-masterkey'
          value: meilisearch_masterkey_value
        }
        {
          name: 'jwt--signingkey'
          value: jwt_signing_key_value
        }
        {
          name: 'mediatr--licensekey'
          value: mediatr_license_key_value
        }
        {
          name: 'reindex--triggerkey'
          value: reindex_trigger_key_value
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: int(api_containerport)
        transport: 'http'
      }
      registries: [
        {
          server: aca_env_outputs_azure_container_registry_endpoint
          identity: aca_env_outputs_azure_container_registry_managed_identity_id
        }
      ]
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
              secretRef: 'connectionstrings--meilisearch'
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
              name: 'MEILISEARCH_MASTERKEY'
              secretRef: 'meilisearch-masterkey'
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
              name: 'Reindex__TriggerKey'
              secretRef: 'reindex--triggerkey'
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
      '${aca_env_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}