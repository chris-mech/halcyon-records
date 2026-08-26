using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Auth.Register;

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "firstName": "John",
                "lastName": "Doe",
                "email": "john.doe@example.com",
                "password": "P@ssw0rd123!"
            }
            """
        )!;
}
