using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;
using HalcyonRecords.Api.Domain;

namespace HalcyonRecords.Api.Features.Orders.GetOrders;

public sealed record OrderSummaryResponse(
    string OrderNumber,
    DateTimeOffset PlacedAt,
    OrderStatus Status,
    int TotalInPence,
    IReadOnlyList<OrderSummaryItemResponse> Items
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "orderNumber": "HR-284917",
                "placedAt": "2026-01-14T16:32:00Z",
                "status": "Placed",
                "totalInPence": 3498,
                "items": [
                    {
                        "albumSqid": "9pXqL2",
                        "title": "Midnight Static",
                        "titleSlug": "midnight-static",
                        "imageUrl": "https://cdn.halcyonrecords.example/albums/midnight-static.jpg"
                    }
                ]
            }
            """
        )!;
}

public sealed record OrderSummaryItemResponse(
    string AlbumSqid,
    string Title,
    string TitleSlug,
    string? ImageUrl
);
