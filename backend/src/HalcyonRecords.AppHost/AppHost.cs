var builder = DistributedApplication.CreateBuilder(args);

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

var keyVault = builder.AddAzureKeyVault("keyvault");
keyVault.AddSecret("kv-jwt-signing-key", "jwt-signing-key", jwtSigningKey.Resource);
keyVault.AddSecret("kv-mediatr-license-key", "mediatr-license-key", mediatrLicenseKey.Resource);

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
        }
    );

keyVault.AddSecret(
    "kv-meilisearch-master-key",
    "meilisearch-master-key",
    meilisearch.Resource.MasterKeyParameter
);

var api = builder
    .AddProject<Projects.HalcyonRecords_Api>("api")
    .WithExternalHttpEndpoints()
    .WithReference(sql)
    .WaitFor(sql)
    .WithReference(meilisearch)
    .WaitFor(meilisearch)
    .WithEnvironment("Jwt__SigningKey", keyVault.GetSecret("jwt-signing-key"))
    .WithEnvironment("MediatR__LicenseKey", keyVault.GetSecret("mediatr-license-key"))
    .WithEnvironment("Meilisearch__MasterKey", keyVault.GetSecret("meilisearch-master-key"))
    .PublishAsAzureContainerApp(
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
