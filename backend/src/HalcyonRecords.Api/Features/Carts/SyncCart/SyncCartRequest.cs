using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Carts.SyncCart;

public sealed record SyncCartRequest(IReadOnlyList<SyncCartItemRequest> Items)
    : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "items": [
                    { "albumSqid": "9pXqL2", "quantity": 2 },
                    { "albumSqid": "7bWn4R", "quantity": 1 }
                ]
            }
            """
        )!;
}

public sealed record SyncCartItemRequest(string AlbumSqid, int Quantity);
