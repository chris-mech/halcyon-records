using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;

namespace HalcyonRecords.Api.Features.Auth.Refresh;

public sealed record RefreshRequest(string RefreshToken) : IOpenApiExampleProvider
{
    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "refreshToken": "8f14e45f-ceea-4b6a-b8b7-4e2a1f6f1a2b"
            }
            """
        )!;
}
