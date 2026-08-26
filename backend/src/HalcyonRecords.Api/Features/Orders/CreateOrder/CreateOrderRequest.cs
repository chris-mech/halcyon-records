using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Orders.CreateOrder;

public sealed record CreateOrderRequest(
    string ContactFirstName,
    string ContactLastName,
    string ContactEmail,
    Guid IdempotencyKey
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "contactFirstName": "John",
                "contactLastName": "Doe",
                "contactEmail": "john.doe@example.com",
                "idempotencyKey": "b3f1c2d4-5e6f-4a7b-8c9d-0e1f2a3b4c5d"
            }
            """
        )!;
}
