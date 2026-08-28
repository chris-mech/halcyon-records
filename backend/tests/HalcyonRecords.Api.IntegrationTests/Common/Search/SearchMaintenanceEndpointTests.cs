using System.Net;
using FluentAssertions;

namespace HalcyonRecords.Api.IntegrationTests.Common.Search;

public class SearchMaintenanceEndpointTests(
    SqlServerContainerFixture sqlFixture,
    MeilisearchContainerFixture meilisearchFixture
) : IAsyncLifetime
{
    private const string TriggerKey = "integration-test-reindex-key";
    private readonly ApiWebApplicationFactory _factory = new(sqlFixture, meilisearchFixture);

    public async ValueTask InitializeAsync()
    {
        await sqlFixture.ResetAsync();
        await meilisearchFixture.ResetAsync();
        Environment.SetEnvironmentVariable("Reindex__TriggerKey", TriggerKey);
    }

    public async ValueTask DisposeAsync()
    {
        Environment.SetEnvironmentVariable("Reindex__TriggerKey", null);
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Post_NoTriggerKeyHeader_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsync(
            "/api/maintenance/search/reindex",
            null,
            TestContext.Current.CancellationToken
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_WrongTriggerKeyHeader_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/maintenance/search/reindex"
        );
        request.Headers.Add("X-Reindex-Key", "not-the-right-key");

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_CorrectTriggerKeyHeader_RebuildsIndex()
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/maintenance/search/reindex"
        );
        request.Headers.Add("X-Reindex-Key", TriggerKey);

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var settings = await meilisearchFixture
            .Client.Index(MeilisearchContainerFixture.IndexName)
            .GetSettingsAsync(TestContext.Current.CancellationToken);
        settings.SearchableAttributes.Should().Contain("title");
    }
}
