using HalcyonRecords.Api.Infrastructure.Search;
using Meilisearch;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.IntegrationTests.Common;

public abstract class SearchIntegrationTestBase(
    SqlServerContainerFixture sqlFixture,
    MeilisearchContainerFixture meilisearchFixture
) : IntegrationTestBase(sqlFixture)
{
    protected MeilisearchClient MeilisearchClient => meilisearchFixture.Client;

    protected MeilisearchIndexer Indexer { get; } =
        new(meilisearchFixture.Client, Options.Create(new SearchOptions()));

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await meilisearchFixture.ResetAsync();
    }
}
