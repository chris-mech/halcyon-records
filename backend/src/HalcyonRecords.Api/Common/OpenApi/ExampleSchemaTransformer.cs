using System.Reflection;
using System.Text.Json.Nodes;
using HalcyonRecords.Api.Common.Contracts;
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
}
