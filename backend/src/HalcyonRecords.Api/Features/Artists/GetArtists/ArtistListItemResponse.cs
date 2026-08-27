using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Artists.GetArtists;

public sealed record ArtistListItemResponse(
    string Sqid,
    string Name,
    string NameSlug,
    int AlbumCount
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "sqid": "4mTb7K",
                "name": "The Coast Runners",
                "nameSlug": "the-coast-runners",
                "albumCount": 2
            }
            """
        )!;
}
