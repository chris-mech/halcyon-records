@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource sqlServerAdminManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: take('sql-admin-${uniqueString(resourceGroup().id)}', 63)
  location: location
}

resource sql 'Microsoft.Sql/servers@2023-08-01' = {
  name: take('sql-${uniqueString(resourceGroup().id)}', 63)
  location: location
  properties: {
    administrators: {
      administratorType: 'ActiveDirectory'
      login: sqlServerAdminManagedIdentity.name
      sid: sqlServerAdminManagedIdentity.properties.principalId
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: true
    }
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    version: '12.0'
  }
  tags: {
    'aspire-resource-name': 'sql'
  }
}

resource sqlFirewallRule_AllowAllAzureIps 'Microsoft.Sql/servers/firewallRules@2023-08-01' = {
  name: 'AllowAllAzureIps'
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
  parent: sql
}

resource halcyonrecords 'Microsoft.Sql/servers/databases@2023-08-01' = {
  name: 'halcyonrecords'
  location: location
  properties: {
    autoPauseDelay: 15
    collation: 'Latin1_General_100_CI_AS_SC'
    freeLimitExhaustionBehavior: 'AutoPause'
    minCapacity: json('0.5')
    useFreeLimit: true
  }
  sku: {
    name: 'GP_S_Gen5_2'
  }
  parent: sql
}

output sqlServerFqdn string = sql.properties.fullyQualifiedDomainName

output name string = sql.name

output id string = sql.id

output sqlServerAdminName string = sql.properties.administrators.login
