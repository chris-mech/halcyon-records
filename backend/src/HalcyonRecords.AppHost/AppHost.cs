var builder = DistributedApplication.CreateBuilder(args);

var sql = builder
    .AddSqlServer("sql")
    .WithImageTag("2025-latest")
    .WithDataVolume()
    .AddDatabase("halcyonrecords");

var meilisearch = builder.AddMeilisearch("meilisearch");

var api = builder
    .AddProject<Projects.HalcyonRecords_Api>("api")
    .WithReference(sql)
    .WithReference(meilisearch);

#pragma warning disable ASPIREJAVASCRIPT001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var frontend = builder.AddNextJsApp("frontend", "../../../frontend").WithBun().WithReference(api).WaitFor(api);
#pragma warning restore ASPIREJAVASCRIPT001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

await builder.Build().RunAsync();
