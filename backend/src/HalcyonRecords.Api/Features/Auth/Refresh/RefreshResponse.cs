using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Auth.Refresh;

public sealed record RefreshResponse(
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
                "refreshToken": "9a25f56g-dfeb-5c7b-c9c8-5f3b2g7g2b3c",
                "expiresAt": "2026-01-14T17:32:00Z"
            }
            """
        )!;
}
