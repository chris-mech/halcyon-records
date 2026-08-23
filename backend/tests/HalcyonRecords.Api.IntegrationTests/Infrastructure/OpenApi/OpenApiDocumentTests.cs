using System.Net.Http.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using HalcyonRecords.Api.Features.Albums.GetAlbums;
using HalcyonRecords.Api.Features.Artists.GetArtistById;
using HalcyonRecords.Api.IntegrationTests.Common;

namespace HalcyonRecords.Api.IntegrationTests.Infrastructure.OpenApi;

public class OpenApiDocumentTests(SqlServerContainerFixture fixture) : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory = new(fixture);

    public ValueTask InitializeAsync() => new(fixture.ResetAsync());

    public async ValueTask DisposeAsync() => await _factory.DisposeAsync();

    [Fact]
    public async Task Document_AlbumsSortParameter_HasEnumSchemaMatchingAlbumSortBy()
    {
        var sortSchema = await GetSortParameterSchemaAsync("/api/albums");

        sortSchema!["type"]!.GetValue<string>().Should().Be("string");
        sortSchema["enum"]!
            .AsArray()
            .Select(v => v!.GetValue<string>())
            .Should()
            .BeEquivalentTo(Enum.GetNames<AlbumSortBy>());
    }

    [Fact]
    public async Task Document_ArtistByIdSortParameter_HasEnumSchemaMatchingArtistAlbumSortBy()
    {
        var sortSchema = await GetSortParameterSchemaAsync("/api/artists/{sqid}");

        sortSchema!["type"]!.GetValue<string>().Should().Be("string");
        sortSchema["enum"]!
            .AsArray()
            .Select(v => v!.GetValue<string>())
            .Should()
            .BeEquivalentTo(Enum.GetNames<ArtistAlbumSortBy>());
    }

    private async Task<JsonNode?> GetSortParameterSchemaAsync(string path)
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        return document!
            ["paths"]
            ?[path]?["get"]?["parameters"]?.AsArray()
            .FirstOrDefault(p => p?["name"]?.GetValue<string>() == "sort")
            ?["schema"];
    }
}
