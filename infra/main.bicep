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

@description('Container image reference for the API, e.g. ghcr.io/<owner>/halcyon-records/api:sha-xxxxxxx')
param api_containerimage string

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
    location: location
    userPrincipalId: principalId
  }
}
module api 'api/api-containerapp.module.bicep' = {
  name: 'api'
  scope: rg
  params: {
    location: location
    aca_env_outputs_azure_container_apps_environment_default_domain: aca_env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
    aca_env_outputs_azure_container_apps_environment_id: aca_env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID
    api_containerimage: api_containerimage
    api_identity_outputs_id: api_identity.outputs.id
    api_containerport: '8080'
    sql_outputs_sqlserverfqdn: sql.outputs.sqlServerFqdn
    meilisearch_masterkey_value: meilisearch_masterKey
    jwt_signing_key_value: jwt_signing_key
    mediatr_license_key_value: mediatr_license_key
    api_identity_outputs_clientid: api_identity.outputs.clientId
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
module meilisearch 'meilisearch/meilisearch-containerapp.module.bicep' = {
  name: 'meilisearch'
  scope: rg
  params: {
    location: location
    aca_env_outputs_azure_container_apps_environment_default_domain: aca_env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
    aca_env_outputs_azure_container_apps_environment_id: aca_env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID
    meilisearch_masterkey_value: meilisearch_masterKey
    aca_env_outputs_volumes_meilisearch_0: aca_env.outputs.volumes_meilisearch_0
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
output ACA_ENV_VOLUMES_MEILISEARCH_0 string = aca_env.outputs.volumes_meilisearch_0
output API_FQDN string = api.outputs.fqdn
output API_IDENTITY_CLIENTID string = api_identity.outputs.clientId
output API_IDENTITY_ID string = api_identity.outputs.id
output AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = aca_env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
output SQL_SQLSERVERFQDN string = sql.outputs.sqlServerFqdn
