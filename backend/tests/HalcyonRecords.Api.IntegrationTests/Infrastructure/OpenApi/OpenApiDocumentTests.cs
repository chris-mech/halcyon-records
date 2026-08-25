using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Features.Albums.GetAlbums;
using HalcyonRecords.Api.Features.Artists.GetArtistById;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.AspNetCore.Http;

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

    [Fact]
    public async Task Document_ArtistsOperation_HasGlobalErrorResponsesFromDocumentTransformer()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        var responses = document!["paths"]!["/api/artists"]!["get"]!["responses"]!.AsObject();

        var expectedErrorStatusCodes =
            GlobalErrorResponseDocumentTransformer.DocumentedStatusCodes.Where(statusCode =>
                statusCode != StatusCodes.Status415UnsupportedMediaType
            );

        responses
            .Select(entry => entry.Key)
            .Should()
            .BeEquivalentTo(
                expectedErrorStatusCodes
                    .Select(statusCode => statusCode.ToString(CultureInfo.InvariantCulture))
                    .Append("200")
            );

        foreach (var statusCode in expectedErrorStatusCodes)
        {
            responses[statusCode.ToString(CultureInfo.InvariantCulture)]!["content"]!
                ["application/problem+json"]
                .Should()
                .NotBeNull();
        }
    }

    [Fact]
    public async Task Document_CreateOrderOperation_IncludesUnsupportedMediaTypeSinceItHasARequestBody()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        document!["paths"]!["/api/orders"]!["post"]!["responses"]!["415"].Should().NotBeNull();
    }

    [Fact]
    public async Task Document_ArtistByIdOperation_PreservesValidationProblemsOwnBadRequestSchema()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        var badRequestSchemaRef = document!["paths"]!["/api/artists/{sqid}"]!["get"]!["responses"]![
            "400"
        ]!["content"]!["application/problem+json"]!["schema"]!["$ref"]!.GetValue<string>();

        badRequestSchemaRef.Should().EndWith("HttpValidationProblemDetails");
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
