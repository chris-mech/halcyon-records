using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Genres.GetGenreBySlug;

public sealed record GenreDetailResponse(
    string Name,
    string Slug,
    string? Description,
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
                "description": "Hazy, reverb-drenched guitar pop that prioritises atmosphere over hooks.",
                "imageUrl": "https://cdn.halcyonrecords.example/genres/dream-pop.jpg",
                "albumCount": 128
            }
            """
        )!;
}
