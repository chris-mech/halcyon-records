using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Decades.GetDecades;

public sealed record DecadeListItemResponse(
    string Slug,
    string Label,
    int? StartYear,
    int? EndYear,
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
                "imageUrl": "https://cdn.halcyonrecords.example/decades/1990s.jpg",
                "albumCount": 842
            }
            """
        )!;
}
