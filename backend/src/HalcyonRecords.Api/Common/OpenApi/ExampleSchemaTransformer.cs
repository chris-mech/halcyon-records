using System.Reflection;
using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HalcyonRecords.Api.Common.OpenApi;

public sealed class ExampleSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var type = context.JsonTypeInfo.Type;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PagedResult<>))
        {
            ApplyPagedExample(schema, type);
            return Task.CompletedTask;
        }

        if (type == typeof(ProblemDetails))
        {
            schema.Examples = [ProblemDetailsExample];
            return Task.CompletedTask;
        }

        if (type == typeof(HttpValidationProblemDetails))
        {
            schema.Examples = [HttpValidationProblemDetailsExample];
            return Task.CompletedTask;
        }

        if (TryGetExample(type, out var example))
        {
            schema.Examples = [example];
        }

        return Task.CompletedTask;
    }

    private static void ApplyPagedExample(OpenApiSchema schema, Type pagedResultType)
    {
        var itemType = pagedResultType.GetGenericArguments()[0];

        if (!TryGetExample(itemType, out var itemExample))
        {
            return;
        }

        schema.Examples =
        [
            new JsonObject
            {
                ["items"] = new JsonArray(itemExample.DeepClone(), itemExample.DeepClone()),
                ["page"] = 1,
                ["pageSize"] = 12,
                ["totalCount"] = 2,
            },
        ];
    }

    private static bool TryGetExample(Type type, out JsonNode example)
    {
        example = null!;

        if (!typeof(IOpenApiExampleProvider).IsAssignableFrom(type))
        {
            return false;
        }

        var exampleProperty = type.GetProperty(
            nameof(IOpenApiExampleProvider.Example),
            BindingFlags.Public | BindingFlags.Static
        );

        if (exampleProperty?.GetValue(null) is not JsonNode value)
        {
            return false;
        }

        example = value;
        return true;
    }

    private static JsonNode ProblemDetailsExample =>
        JsonNode.Parse(
            """
            {
                "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                "title": "Bad Request",
                "status": 400,
                "detail": "The request did not meet the server's requirements and could not be processed.",
                "instance": "/api/albums",
                "traceId": "00-4f8c9b2e1a3d4c5e8f7a6b5c4d3e2f1a-9b8c7d6e5f4a3b2c-00"
            }
            """
        )!;

    private static JsonNode HttpValidationProblemDetailsExample =>
        JsonNode.Parse(
            """
            {
                "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                "title": "One or more validation errors occurred.",
                "status": 400,
                "detail": null,
                "instance": "/api/auth/register",
                "traceId": "00-4f8c9b2e1a3d4c5e8f7a6b5c4d3e2f1a-9b8c7d6e5f4a3b2c-00",
                "errors": {
                    "Email": ["'Email' is not a valid email address."]
                }
            }
            """
        )!;
}
