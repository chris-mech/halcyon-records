using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Auth.Login;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzZmE4NWY2NCJ9.sIgnAtUrE",
                "refreshToken": "8f14e45f-ceea-4b6a-b8b7-4e2a1f6f1a2b",
                "expiresAt": "2026-01-14T17:32:00Z"
            }
            """
        )!;
}
