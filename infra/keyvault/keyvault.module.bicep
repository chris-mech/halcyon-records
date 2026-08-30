@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

@secure()
param jwt_signing_key_value string

@secure()
param mediatr_license_key_value string

@secure()
param meilisearch_masterkey_value string

resource keyvault 'Microsoft.KeyVault/vaults@2024-11-01' = {
  name: take('keyvault-${uniqueString(resourceGroup().id)}', 24)
  location: location
  properties: {
    tenantId: tenant().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
  }
  tags: {
    'aspire-resource-name': 'keyvault'
  }
}

resource secret_jwt_signing_key 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  name: 'jwt-signing-key'
  properties: {
    value: jwt_signing_key_value
  }
  parent: keyvault
}

resource secret_mediatr_license_key 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  name: 'mediatr-license-key'
  properties: {
    value: mediatr_license_key_value
  }
  parent: keyvault
}

resource secret_meilisearch_master_key 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  name: 'meilisearch-master-key'
  properties: {
    value: meilisearch_masterkey_value
  }
  parent: keyvault
}

output vaultUri string = keyvault.properties.vaultUri

output name string = keyvault.name

output id string = keyvault.id