using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Orders.CreateOrder;

public sealed record CreateOrderResponse(
    string OrderNumber,
    DateTimeOffset PlacedAt,
    int TotalInPence,
    IReadOnlyList<CreateOrderItemResponse> Items
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "orderNumber": "HR-284917",
                "placedAt": "2026-01-14T16:32:00Z",
                "totalInPence": 3498,
                "items": [
                    {
                        "albumSqid": "9pXqL2",
                        "title": "Midnight Static",
                        "titleSlug": "midnight-static",
                        "imageUrl": "https://cdn.halcyonrecords.example/albums/midnight-static.jpg",
                        "quantity": 2,
                        "priceAtPurchaseInPence": 1899
                    }
                ]
            }
            """
        )!;
}

public sealed record CreateOrderItemResponse(
    string AlbumSqid,
    string Title,
    string TitleSlug,
    string? ImageUrl,
    int Quantity,
    int PriceAtPurchaseInPence
);
