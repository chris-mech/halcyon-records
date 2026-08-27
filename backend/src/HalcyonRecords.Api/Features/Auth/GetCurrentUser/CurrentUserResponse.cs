using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Auth.GetCurrentUser;

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTimeOffset RegisteredAt
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                "email": "john.doe@example.com",
                "firstName": "John",
                "lastName": "Doe",
                "registeredAt": "2024-11-02T09:15:00Z"
            }
            """
        )!;
}
