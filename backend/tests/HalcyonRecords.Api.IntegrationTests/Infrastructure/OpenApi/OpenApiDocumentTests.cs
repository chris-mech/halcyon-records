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
    public async Task Document_OpenApiVersion_Is3_1()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        document!["openapi"]!.GetValue<string>().Should().StartWith("3.1");
    }

    [Fact]
    public async Task Document_ArtistDetailResponseSinceYear_UsesTypeArrayForNullableIntegerUnderOpenApi31()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        var sinceYearType = document!["components"]!["schemas"]!["ArtistDetailResponse"]![
            "properties"
        ]!["sinceYear"]!["type"]!;

        sinceYearType
            .AsArray()
            .Select(t => t!.GetValue<string>())
            .Should()
            .BeEquivalentTo("integer", "null");
    }

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
    public async Task Document_AlbumsSortParameter_HasDefaultDespiteSortEnumParameterTransformerReplacingSchema()
    {
        var sortSchema = await GetSortParameterSchemaAsync("/api/albums");

        sortSchema!["default"]!.GetValue<string>().Should().Be(nameof(AlbumSortBy.NewestFirst));
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

    [Fact]
    public async Task Document_BearerSecurityScheme_HasCorrectShape()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        var bearerScheme = document!["components"]!["securitySchemes"]!["Bearer"]!;

        bearerScheme["type"]!.GetValue<string>().Should().Be("http");
        bearerScheme["scheme"]!.GetValue<string>().Should().Be("bearer");
        bearerScheme["bearerFormat"]!.GetValue<string>().Should().Be("JWT");
    }

    [Fact]
    public async Task Document_GetCartOperation_RequiresBearerSecurity()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        var security = document!["paths"]!["/api/cart"]!["get"]!["security"]!.AsArray();

        security.Should().ContainSingle();
        security[0]!.AsObject().ContainsKey("Bearer").Should().BeTrue();
    }

    [Fact]
    public async Task Document_ArtistsOperation_HasNoSecurityRequirementSinceItIsPublic()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        document!["paths"]!["/api/artists"]!["get"]!["security"].Should().BeNull();
    }

    [Fact]
    public async Task Document_AlbumDetailResponseSchema_HasExampleFromExampleSchemaTransformer()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        var example = document!["components"]!["schemas"]!["AlbumDetailResponse"]!["examples"]!
            .AsArray()
            .Single()!;

        example["sqid"]!.GetValue<string>().Should().Be("9pXqL2");
        example["title"]!.GetValue<string>().Should().Be("Midnight Static");
    }

    [Fact]
    public async Task Document_GetAlbumsOperationResponseSchema_HasSynthesizedPagedExample()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        var responseSchema = document!["paths"]!["/api/albums"]!["get"]!["responses"]!["200"]![
            "content"
        ]!["application/json"]!["schema"]!;

        var resolvedSchema = ResolveSchema(document, responseSchema);
        var example = resolvedSchema["examples"]!.AsArray().Single()!;

        example["page"]!.GetValue<int>().Should().Be(1);
        example["items"]!.AsArray().Should().HaveCount(2);
        example["items"]![0]!["sqid"]!.GetValue<string>().Should().Be("9pXqL2");
    }

    [Fact]
    public async Task Document_GetAlbumsGenresParameter_HasDescriptionFromDescriptionAttribute()
    {
        var parameter = await GetParameterAsync("/api/albums", "genres");

        parameter!["description"]!
            .GetValue<string>()
            .Should()
            .Be("Genre slugs to filter by. An album matching any of the given genres is included.");
    }

    [Fact]
    public async Task Document_GetAlbumsGenresParameter_HasExampleFromOperationTransformer()
    {
        var parameter = await GetParameterAsync("/api/albums", "genres");

        parameter!["example"]!
            .AsArray()
            .Select(v => v!.GetValue<string>())
            .Should()
            .BeEquivalentTo("shoegaze", "dream-pop");
    }

    [Fact]
    public async Task Document_RefreshOperation_HasDescriptionFromWithDescription()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        document!["paths"]!["/api/auth/refresh"]!["post"]!["description"]!
            .GetValue<string>()
            .Should()
            .StartWith("Rotates the refresh token");
    }

    [Fact]
    public async Task Document_HttpValidationProblemDetailsSchema_HasNoExampleSinceItDoesNotImplementTheProvider()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        document!["components"]!["schemas"]!["HttpValidationProblemDetails"]!
            ["examples"]
            .Should()
            .BeNull();
    }

    [Fact]
    public async Task Document_GetSearchSuggestionsOperationResponseSchema_HasExampleFromOperationTransformer()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        var example = document!["paths"]!["/api/search/suggestions"]!["get"]!["responses"]!["200"]![
            "content"
        ]!["application/json"]!["schema"]!["examples"]!
            .AsArray()
            .Single()!;

        example
            .AsArray()
            .Select(v => v!.GetValue<string>())
            .Should()
            .BeEquivalentTo("Midnight Static", "Dream Pop", "Shoegaze");
    }

    [Fact]
    public async Task Document_RegisterRequestEmailProperty_HasEmailFormatFromWellKnownPropertyFormatTransformer()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        document!["components"]!["schemas"]!["RegisterRequest"]!["properties"]!["email"]!["format"]!
            .GetValue<string>()
            .Should()
            .Be("email");
    }

    [Fact]
    public async Task Document_AlbumSummaryResponseImageUrlProperty_HasUriFormatFromWellKnownPropertyFormatTransformer()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        document!["components"]!["schemas"]!["AlbumSummaryResponse"]!["properties"]!["imageUrl"]![
            "format"
        ]!
            .GetValue<string>()
            .Should()
            .Be("uri");
    }

    [Fact]
    public async Task Document_RegisterRequestPasswordProperty_HasMinLengthAndPatternFromPasswordPolicyTransformer()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        var passwordSchema = document!["components"]!["schemas"]!["RegisterRequest"]![
            "properties"
        ]!["password"]!;

        passwordSchema["minLength"]!.GetValue<int>().Should().Be(6);
        passwordSchema["pattern"]!
            .GetValue<string>()
            .Should()
            .Be("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z0-9]).+$");
    }

    [Fact]
    public async Task Document_LoginRequestPasswordProperty_HasNoPatternSincePolicyOnlyAppliesToRegistration()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        document!["components"]!["schemas"]!["LoginRequest"]!["properties"]!["password"]!
            ["pattern"]
            .Should()
            .BeNull();
    }

    [Fact]
    public async Task Document_SyncCartItemRequestQuantityProperty_HasMinimumButNoMaximum()
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        var quantitySchema = document!["components"]!["schemas"]!["SyncCartItemRequest"]![
            "properties"
        ]!["quantity"]!;

        quantitySchema["minimum"]!.GetValue<int>().Should().Be(1);
        quantitySchema["maximum"].Should().BeNull();
    }

    private async Task<JsonNode?> GetSortParameterSchemaAsync(string path) =>
        (await GetParameterAsync(path, "sort"))?["schema"];

    private async Task<JsonNode?> GetParameterAsync(string path, string parameterName)
    {
        using var client = _factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonNode>(
            new Uri("/openapi/v1.json", UriKind.Relative),
            TestContext.Current.CancellationToken
        );

        return document!
            ["paths"]
            ?[path]?["get"]?["parameters"]?.AsArray()
            .FirstOrDefault(p => p?["name"]?.GetValue<string>() == parameterName);
    }

    private static JsonNode ResolveSchema(JsonNode document, JsonNode schema)
    {
        var refValue = schema["$ref"]?.GetValue<string>();

        if (refValue is null)
        {
            return schema;
        }

        var schemaName = refValue.Split('/').Last();
        return document["components"]!["schemas"]![schemaName]!;
    }
}
