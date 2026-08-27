using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.OpenApi;
using Microsoft.AspNetCore.Mvc;

namespace HalcyonRecords.Api.Common.Results;

public sealed class DomainProblemDetails : ProblemDetails, IOpenApiExampleProvider
{
    public required string Code { get; init; }

    public static JsonNode Example =>
        JsonNode.Parse(
            """
            {
                "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                "title": "Not Found",
                "status": 404,
                "detail": "Album '9pXqL2' not found.",
                "instance": "/api/albums/9pXqL2",
                "code": "Album.NotFound",
                "traceId": "00-4f8c9b2e1a3d4c5e8f7a6b5c4d3e2f1a-9b8c7d6e5f4a3b2c-00"
            }
            """
        )!;
}
