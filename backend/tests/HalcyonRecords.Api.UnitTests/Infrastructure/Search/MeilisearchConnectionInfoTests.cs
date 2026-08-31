using FluentAssertions;
using HalcyonRecords.Api.Infrastructure.Search;

namespace HalcyonRecords.Api.UnitTests.Infrastructure.Search;

public class MeilisearchConnectionInfoTests
{
    [Fact]
    public void Parse_BareUriConnectionString_HasNullMasterKey()
    {
        var connectionInfo = MeilisearchConnectionInfo.Parse("http://meilisearch.internal:443");

        connectionInfo.Endpoint.Should().Be(new Uri("http://meilisearch.internal:443"));
        connectionInfo.MasterKey.Should().BeNull();
    }

    [Fact]
    public void Parse_CompositeConnectionString_ExtractsMasterKeyFromString()
    {
        var connectionInfo = MeilisearchConnectionInfo.Parse(
            "Endpoint=http://meilisearch.internal:443;MasterKey=from-connection-string"
        );

        connectionInfo.MasterKey.Should().Be("from-connection-string");
    }

    [Fact]
    public void Parse_BareUriConnectionString_OverrideWins()
    {
        var connectionInfo = MeilisearchConnectionInfo.Parse(
            "http://meilisearch.internal:443",
            masterKeyOverride: "from-override"
        );

        connectionInfo.MasterKey.Should().Be("from-override");
    }

    [Fact]
    public void Parse_CompositeConnectionString_OverrideWinsOverEmbeddedKey()
    {
        var connectionInfo = MeilisearchConnectionInfo.Parse(
            "Endpoint=http://meilisearch.internal:443;MasterKey=from-connection-string",
            masterKeyOverride: "from-override"
        );

        connectionInfo.MasterKey.Should().Be("from-override");
    }

    [Fact]
    public void Parse_NullConnectionString_Throws() =>
        FluentActions
            .Invoking(() => MeilisearchConnectionInfo.Parse(null))
            .Should()
            .Throw<InvalidOperationException>();
}
