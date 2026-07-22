var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("halcyonrecords");

var meilisearch = builder.AddMeilisearch("meilisearch");

builder.AddProject<Projects.HalcyonRecords_Api>("api")
    .WithReference(sql)
    .WithReference(meilisearch);

builder.Build().Run();
