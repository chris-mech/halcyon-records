@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param environmentId string

param containerImage string

param identityId string

param identityClientId string

param sqlServerFqdn string

param meilisearchDefaultDomain string

param keyVaultName string

param jobName string

param triggerType string

param cronExpression string = ''

param replicaTimeout int

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: keyVaultName
}

resource keyVaultJwtSigningKey 'Microsoft.KeyVault/vaults/secrets@2024-11-01' existing = {
  name: 'jwt-signing-key'
  parent: keyVault
}

resource keyVaultMediatrLicenseKey 'Microsoft.KeyVault/vaults/secrets@2024-11-01' existing = {
  name: 'mediatr-license-key'
  parent: keyVault
}

resource keyVaultMeilisearchMasterKey 'Microsoft.KeyVault/vaults/secrets@2024-11-01' existing = {
  name: 'meilisearch-master-key'
  parent: keyVault
}

resource job 'Microsoft.App/jobs@2025-07-01' = {
  name: jobName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityId}': {}
    }
  }
  properties: {
    environmentId: environmentId
    configuration: {
      triggerType: triggerType
      replicaTimeout: replicaTimeout
      replicaRetryLimit: 1
      manualTriggerConfig: triggerType == 'Manual' ? { parallelism: 1, replicaCompletionCount: 1 } : null
      scheduleTriggerConfig: triggerType == 'Schedule' ? { cronExpression: cronExpression, parallelism: 1, replicaCompletionCount: 1 } : null
      secrets: [
        {
          name: 'jwt--signingkey'
          identity: identityId
          keyVaultUrl: keyVaultJwtSigningKey.properties.secretUri
        }
        {
          name: 'mediatr--licensekey'
          identity: identityId
          keyVaultUrl: keyVaultMediatrLicenseKey.properties.secretUri
        }
        {
          name: 'meilisearch--masterkey'
          identity: identityId
          keyVaultUrl: keyVaultMeilisearchMasterKey.properties.secretUri
        }
      ]
    }
    template: {
      containers: [
        {
          image: containerImage
          name: jobName
          args: [
            '--job'
            jobName
          ]
          env: [
            {
              name: 'ConnectionStrings__halcyonrecords'
              value: 'Server=tcp:${sqlServerFqdn},1433;Encrypt=True;Authentication="Active Directory Default";Database=halcyonrecords;Connect Timeout=60'
            }
            {
              name: 'ConnectionStrings__meilisearch'
              value: 'https://${'meilisearch.internal.${meilisearchDefaultDomain}'}:443'
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
              value: identityClientId
            }
            {
              name: 'AZURE_TOKEN_CREDENTIALS'
              value: 'ManagedIdentityCredential'
            }
          ]
        }
      ]
    }
  }
}
