@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param aca_env_outputs_azure_container_apps_environment_default_domain string

param aca_env_outputs_azure_container_apps_environment_id string

@secure()
param meilisearch_masterkey_value string

param aca_env_outputs_volumes_meilisearch_0 string

resource meilisearch 'Microsoft.App/containerApps@2025-07-01' = {
  name: 'meilisearch'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'meili-master-key'
          value: meilisearch_masterkey_value
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
}