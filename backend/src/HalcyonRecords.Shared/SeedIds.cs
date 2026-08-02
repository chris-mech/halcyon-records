using System.Text.Json;
using System.Text.Json.Serialization;

namespace HalcyonRecords.Shared;

[JsonConverter(typeof(ArtistMbidJsonConverter))]
public readonly record struct ArtistMbid(Guid Value)
{
    public override string ToString() => Value.ToString();
}

public sealed class ArtistMbidJsonConverter : JsonConverter<ArtistMbid>
{
    public override ArtistMbid Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => new(reader.GetGuid());

    public override void Write(
        Utf8JsonWriter writer,
        ArtistMbid value,
        JsonSerializerOptions options
    ) => writer.WriteStringValue(value.Value);
}

[JsonConverter(typeof(ReleaseMbidJsonConverter))]
public readonly record struct ReleaseMbid(Guid Value)
{
    public override string ToString() => Value.ToString();
}

public sealed class ReleaseMbidJsonConverter : JsonConverter<ReleaseMbid>
{
    public override ReleaseMbid Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => new(reader.GetGuid());

    public override void Write(
        Utf8JsonWriter writer,
        ReleaseMbid value,
        JsonSerializerOptions options
    ) => writer.WriteStringValue(value.Value);
}

[JsonConverter(typeof(GenreSlugJsonConverter))]
public readonly record struct GenreSlug(string Value)
{
    public override string ToString() => Value;
}

public sealed class GenreSlugJsonConverter : JsonConverter<GenreSlug>
{
    public override GenreSlug Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    ) => new(reader.GetString() ?? throw new JsonException("Expected a non-null genre slug."));

    public override void Write(
        Utf8JsonWriter writer,
        GenreSlug value,
        JsonSerializerOptions options
    ) => writer.WriteStringValue(value.Value);
}
