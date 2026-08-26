using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Decades.GetDecadeBySlug;

public sealed record DecadeDetailResponse(
    string Slug,
    string Label,
    int? StartYear,
    int? EndYear,
    string? Description,
    string? ImageUrl,
    int AlbumCount
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "slug": "1990s",
                "label": "1990s",
                "startYear": 1990,
                "endYear": 1999,
                "description": "The decade that gave us shoegaze, trip-hop, and the rise of Britpop.",
                "imageUrl": "https://cdn.halcyonrecords.example/decades/1990s.jpg",
                "albumCount": 842
            }
            """
        )!;
}
