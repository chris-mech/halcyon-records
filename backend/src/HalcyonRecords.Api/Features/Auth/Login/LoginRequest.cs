using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Auth.Login;

public sealed record LoginRequest(string Email, string Password) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "email": "john.doe@example.com",
                "password": "P@ssw0rd123!"
            }
            """
        )!;
}
