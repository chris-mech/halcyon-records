@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_env_outputs_azure_container_apps_environment_default_domain string

param aca_env_outputs_azure_container_apps_environment_id string

param keyvault_outputs_name string

param identityId string

param aca_env_outputs_volumes_meilisearch_0 string

resource keyvault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: keyvault_outputs_name
}

resource keyvault_meilisearch_master_key 'Microsoft.KeyVault/vaults/secrets@2024-11-01' existing = {
  name: 'meilisearch-master-key'
  parent: keyvault
}

resource meilisearch 'Microsoft.App/containerApps@2025-07-01' = {
  name: 'meilisearch'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'meili-master-key'
          identity: identityId
          keyVaultUrl: keyvault_meilisearch_master_key.properties.secretUri
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: false
        targetPort: 7700
        transport: 'http'
      }
    }
    environmentId: aca_env_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          image: 'docker.io/getmeili/meilisearch:v1.21'
          name: 'meilisearch'
          env: [
            {
              name: 'MEILI_MASTER_KEY'
              secretRef: 'meili-master-key'
            }
          ]
          volumeMounts: [
            {
              volumeName: 'v0'
              mountPath: '/meili_data'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
        cooldownPeriod: 120
      }
      volumes: [
        {
          name: 'v0'
          storageType: 'AzureFile'
          storageName: aca_env_outputs_volumes_meilisearch_0
        }
      ]
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityId}': { }
    }
  }
}
