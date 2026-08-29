@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param environmentId string

param containerImage string

param identityId string

param identityClientId string

param sqlServerFqdn string

param meilisearchDefaultDomain string

@secure()
param meilisearchMasterkeyValue string

@secure()
param jwtSigningKeyValue string

@secure()
param mediatrLicenseKeyValue string

param jobName string

param triggerType string

param cronExpression string = ''

param replicaTimeout int

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
          name: 'connectionstrings--meilisearch'
          value: 'Endpoint=http://${'meilisearch.internal.${meilisearchDefaultDomain}'}:443;MasterKey=${meilisearchMasterkeyValue}'
        }
        {
          name: 'jwt--signingkey'
          value: jwtSigningKeyValue
        }
        {
          name: 'mediatr--licensekey'
          value: mediatrLicenseKeyValue
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
              value: 'Server=tcp:${sqlServerFqdn},1433;Encrypt=True;Authentication="Active Directory Default";Database=halcyonrecords'
            }
            {
              name: 'ConnectionStrings__meilisearch'
              secretRef: 'connectionstrings--meilisearch'
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
