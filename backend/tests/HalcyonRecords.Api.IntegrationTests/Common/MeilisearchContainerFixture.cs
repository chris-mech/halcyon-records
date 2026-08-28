using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Meilisearch;

[assembly: AssemblyFixture(
    typeof(HalcyonRecords.Api.IntegrationTests.Common.MeilisearchContainerFixture)
)]

namespace HalcyonRecords.Api.IntegrationTests.Common;

public sealed class MeilisearchContainerFixture : IAsyncLifetime
{
    private const int MeilisearchPort = 7700;
    private const string MasterKey = "test-master-key";
    public const string IndexName = "albums";

    private readonly IContainer _container = new ContainerBuilder("getmeili/meilisearch:v1.21")
        .WithPortBinding(MeilisearchPort, assignRandomHostPort: true)
        .WithEnvironment("MEILI_MASTER_KEY", MasterKey)
        .WithWaitStrategy(
            Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(request =>
                    request.ForPort(MeilisearchPort).ForPath("/health")
                )
        )
        .Build();

    public MeilisearchClient Client { get; private set; } = null!;
    public string ConnectionString { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var endpoint =
            $"http://{_container.Hostname}:{_container.GetMappedPublicPort(MeilisearchPort)}";
        Client = new MeilisearchClient(endpoint, MasterKey);
        ConnectionString = $"Endpoint={endpoint};MasterKey={MasterKey}";

        var createTask = await Client.CreateIndexAsync(IndexName, "id");
        await Client.Index(IndexName).WaitForTaskAsync(createTask.TaskUid);
    }

    public async Task ResetAsync()
    {
        var index = Client.Index(IndexName);
        var deleteTask = await index.DeleteAllDocumentsAsync();
        await index.WaitForTaskAsync(deleteTask.TaskUid);
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}
