using System.Text.Json;
using System.Text.Json.Serialization;

namespace HalcyonRecords.SeedDataGenerator.Core.Common;

[JsonConverter(typeof(ReleaseGroupMbidJsonConverter))]
public readonly record struct ReleaseGroupMbid(Guid Value)
{
    public override string ToString() => Value.ToString();
}

public sealed class ReleaseGroupMbidJsonConverter : JsonConverter<ReleaseGroupMbid>
{
    public override ReleaseGroupMbid Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => new(reader.GetGuid());

    public override void Write(
        Utf8JsonWriter writer,
        ReleaseGroupMbid value,
        JsonSerializerOptions options
    ) => writer.WriteStringValue(value.Value);
}

[JsonConverter(typeof(DiscogsArtistIdJsonConverter))]
public readonly record struct DiscogsArtistId(long Value)
{
    public override string ToString() => Value.ToString();
}

public sealed class DiscogsArtistIdJsonConverter : JsonConverter<DiscogsArtistId>
{
    public override DiscogsArtistId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => new(reader.GetInt64());

    public override void Write(
        Utf8JsonWriter writer,
        DiscogsArtistId value,
        JsonSerializerOptions options
    ) => writer.WriteNumberValue(value.Value);
}

[JsonConverter(typeof(DiscogsMasterIdJsonConverter))]
public readonly record struct DiscogsMasterId(long Value)
{
    public override string ToString() => Value.ToString();
}

public sealed class DiscogsMasterIdJsonConverter : JsonConverter<DiscogsMasterId>
{
    public override DiscogsMasterId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => new(reader.GetInt64());

    public override void Write(
        Utf8JsonWriter writer,
        DiscogsMasterId value,
        JsonSerializerOptions options
    ) => writer.WriteNumberValue(value.Value);
}

[JsonConverter(typeof(WikidataQidJsonConverter))]
public readonly record struct WikidataQid(string Value)
{
    public override string ToString() => Value;
}

public sealed class WikidataQidJsonConverter : JsonConverter<WikidataQid>
{
    public override WikidataQid Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => new(reader.GetString() ?? throw new JsonException("Expected a non-null Wikidata Q-id."));

    public override void Write(
        Utf8JsonWriter writer,
        WikidataQid value,
        JsonSerializerOptions options
    ) => writer.WriteStringValue(value.Value);
}
