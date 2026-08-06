var builder = DistributedApplication.CreateBuilder(args);

var sql = builder
    .AddSqlServer("sql")
    .WithImageTag("2025-latest")
    .WithDataVolume()
    .AddDatabase("halcyonrecords");

var meilisearch = builder.AddMeilisearch("meilisearch");

builder
    .AddProject<Projects.HalcyonRecords_Api>("api")
    .WithReference(sql)
    .WithReference(meilisearch);

await builder.Build().RunAsync();
