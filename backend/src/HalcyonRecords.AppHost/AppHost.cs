using Aspire.Hosting.Azure;

var builder = DistributedApplication.CreateBuilder(args);
var isPublishMode = builder.ExecutionContext.IsPublishMode;

builder.AddAzureContainerAppEnvironment("aca-env");

var jwtSigningKey = builder.AddParameter(
    "jwt-signing-key",
    new GenerateParameterDefault { MinLength = 64 },
    secret: true,
    persist: true
);
var mediatrLicenseKey = builder.AddParameter("mediatr-license-key", secret: true);
var authSecret = builder.AddParameter(
    "auth-secret",
    new GenerateParameterDefault { MinLength = 32 },
    secret: true,
    persist: true
);

IResourceBuilder<AzureKeyVaultResource>? keyVault = isPublishMode
    ? builder.AddAzureKeyVault("keyvault")
    : null;
keyVault?.AddSecret("kv-jwt-signing-key", "jwt-signing-key", jwtSigningKey.Resource);
keyVault?.AddSecret("kv-mediatr-license-key", "mediatr-license-key", mediatrLicenseKey.Resource);

IResourceBuilder<AzureApplicationInsightsResource>? appInsights = isPublishMode
    ? builder.AddAzureApplicationInsights("appinsights")
    : null;

var sql = builder
    .AddAzureSqlServer("sql")
    .RunAsContainer(c => c.WithImageTag("2025-latest").WithDataVolume())
    .AddDatabase("halcyonrecords");

var meilisearch = builder
    .AddMeilisearch("meilisearch")
    .WithDataVolume()
    .PublishAsAzureContainerApp(
        (infrastructure, app) =>
        {
            app.Template.Scale.MinReplicas = 0;
            app.Template.Scale.MaxReplicas = 1;
            app.Template.Scale.CooldownPeriod = 120;
        }
    );

keyVault?.AddSecret(
    "kv-meilisearch-master-key",
    "meilisearch-master-key",
    meilisearch.Resource.MasterKeyParameter
);

#pragma warning disable ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var api = builder
    .AddProject<Projects.HalcyonRecords_Api>("api")
    .WithExternalHttpEndpoints()
    .WithoutHttpsCertificate()
    .WithReference(sql)
    .WaitFor(sql)
    .WithReference(meilisearch)
    .WaitFor(meilisearch);
#pragma warning restore ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

if (appInsights is not null)
{
    api = api.WithReference(appInsights);
}

api = isPublishMode
    ? api.WithEnvironment("Jwt__SigningKey", keyVault!.GetSecret("jwt-signing-key"))
        .WithEnvironment("MediatR__LicenseKey", keyVault!.GetSecret("mediatr-license-key"))
        .WithEnvironment("Meilisearch__MasterKey", keyVault!.GetSecret("meilisearch-master-key"))
    : api.WithEnvironment("Jwt__SigningKey", jwtSigningKey)
        .WithEnvironment("MediatR__LicenseKey", mediatrLicenseKey);

api = api.PublishAsAzureContainerApp(
    (infrastructure, app) =>
    {
        app.Template.Scale.MinReplicas = 0;
        app.Template.Scale.MaxReplicas = 3;
    }
);

#pragma warning disable ASPIREJAVASCRIPT001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
builder
    .AddNextJsApp("frontend", "../../../frontend")
    .WithBun()
    .WithHttpEndpoint(port: 3000)
    .WithReference(api)
    .WaitFor(api)
    .WithEnvironment("AUTH_SECRET", authSecret)
    .ExcludeFromManifest();
#pragma warning restore ASPIREJAVASCRIPT001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

await builder.Build().RunAsync();
