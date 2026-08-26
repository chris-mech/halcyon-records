using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Domain;

namespace HalcyonRecords.Api.Features.Orders.GetOrderByNumber;

public sealed record OrderDetailResponse(
    string OrderNumber,
    DateTimeOffset PlacedAt,
    OrderStatus Status,
    string ContactFirstName,
    string ContactLastName,
    string ContactEmail,
    int TotalInPence,
    IReadOnlyList<OrderDetailItemResponse> Items
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "orderNumber": "HR-284917",
                "placedAt": "2026-01-14T16:32:00Z",
                "status": "Placed",
                "contactFirstName": "John",
                "contactLastName": "Doe",
                "contactEmail": "john.doe@example.com",
                "totalInPence": 3498,
                "items": [
                    {
                        "albumSqid": "9pXqL2",
                        "title": "Midnight Static",
                        "titleSlug": "midnight-static",
                        "artists": [
                            { "sqid": "4mTb7K", "name": "The Coast Runners", "nameSlug": "the-coast-runners" }
                        ],
                        "imageUrl": "https://cdn.halcyonrecords.example/albums/midnight-static.jpg",
                        "quantity": 2,
                        "priceAtPurchaseInPence": 1899
                    }
                ]
            }
            """
        )!;
}

public sealed record OrderDetailItemResponse(
    string AlbumSqid,
    string Title,
    string TitleSlug,
    IReadOnlyList<OrderDetailItemArtistResponse> Artists,
    string? ImageUrl,
    int Quantity,
    int PriceAtPurchaseInPence
);

public sealed record OrderDetailItemArtistResponse(string Sqid, string Name, string NameSlug);
