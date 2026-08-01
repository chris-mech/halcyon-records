using System.Text.Json;
using System.Text.Json.Serialization;

namespace HalcyonRecords.Api.Infrastructure.Seed;

public static class SeedDataFileNames
{
    public const string Artists = "SampleArtists.json";
    public const string Genres = "SampleGenres.json";
    public const string Albums = "SampleAlbums.json";
}

public static class SeedDataJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter<SeedSource>() },
    };
}
