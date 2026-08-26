using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Genres.GetGenres;

public sealed record GenreListItemResponse(
    string Name,
    string Slug,
    string? ImageUrl,
    int AlbumCount
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "name": "Dream Pop",
                "slug": "dream-pop",
                "imageUrl": "https://cdn.halcyonrecords.example/genres/dream-pop.jpg",
                "albumCount": 128
            }
            """
        )!;
}
