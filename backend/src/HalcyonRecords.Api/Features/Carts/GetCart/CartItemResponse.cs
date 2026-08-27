using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Carts.GetCart;

public sealed record CartItemResponse(
    string AlbumSqid,
    string Title,
    string TitleSlug,
    string? ImageUrl,
    int PriceInPence,
    int? OriginalPriceInPence,
    int Quantity,
    int UnitsInStock,
    bool IsInStock,
    IReadOnlyList<CartItemArtistResponse> Artists
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "albumSqid": "9pXqL2",
                "title": "Midnight Static",
                "titleSlug": "midnight-static",
                "imageUrl": "https://cdn.halcyonrecords.example/albums/midnight-static.jpg",
                "priceInPence": 1899,
                "originalPriceInPence": 2299,
                "quantity": 2,
                "unitsInStock": 12,
                "isInStock": true,
                "artists": [
                    { "sqid": "4mTb7K", "name": "The Coast Runners", "nameSlug": "the-coast-runners" }
                ]
            }
            """
        )!;
}

public sealed record CartItemArtistResponse(string Sqid, string Name, string NameSlug);
