targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention, the name of the resource group for your application will use this name, prefixed with rg-')
param environmentName string

@minLength(1)
@description('The location used for all deployed resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string = ''

@metadata({azd: {
  type: 'generate'
  config: {length:32}
  }
})
@secure()
param auth_secret string
@metadata({azd: {
  type: 'generate'
  config: {length:64}
  }
})
@secure()
param jwt_signing_key string
@secure()
param mediatr_license_key string
@metadata({azd: {
  type: 'generate'
  config: {length:22}
  }
})
@secure()
param meilisearch_masterKey string
@metadata({azd: {
  type: 'generate'
  config: {length:32}
  }
})
@secure()
param reindex_trigger_key string

var tags = {
  'azd-env-name': environmentName
}

resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module aca_env 'aca-env/aca-env.module.bicep' = {
  name: 'aca-env'
  scope: rg
  params: {
    aca_env_acr_outputs_name: aca_env_acr.outputs.name
    location: location
    userPrincipalId: principalId
  }
}
module aca_env_acr 'aca-env-acr/aca-env-acr.module.bicep' = {
  name: 'aca-env-acr'
  scope: rg
  params: {
    location: location
  }
}
module api_identity 'api-identity/api-identity.module.bicep' = {
  name: 'api-identity'
  scope: rg
  params: {
    location: location
  }
}
module api_roles_sql 'api-roles-sql/api-roles-sql.module.bicep' = {
  name: 'api-roles-sql'
  scope: rg
  params: {
    location: location
    principalId: api_identity.outputs.principalId
    principalName: api_identity.outputs.principalName
    sql_outputs_name: sql.outputs.name
    sql_outputs_sqlserveradminname: sql.outputs.sqlServerAdminName
  }
}
module sql 'sql/sql.module.bicep' = {
  name: 'sql'
  scope: rg
  params: {
    location: location
  }
}
output ACA_ENV_AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = aca_env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
output ACA_ENV_AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = aca_env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID
output ACA_ENV_AZURE_CONTAINER_REGISTRY_ENDPOINT string = aca_env.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output ACA_ENV_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = aca_env.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID
output ACA_ENV_VOLUMES_MEILISEARCH_0 string = aca_env.outputs.volumes_meilisearch_0
output API_IDENTITY_CLIENTID string = api_identity.outputs.clientId
output API_IDENTITY_ID string = api_identity.outputs.id
output AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = aca_env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = aca_env.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output SQL_SQLSERVERFQDN string = sql.outputs.sqlServerFqdn
