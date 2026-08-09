using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HalcyonRecords.Api.Common.OpenApi;

public sealed class IntegerSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var type = context.JsonTypeInfo.Type;
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (IsIntegerType(underlyingType))
        {
            var isNullable = schema.Type?.HasFlag(JsonSchemaType.Null) ?? false;
            schema.Type = isNullable
                ? JsonSchemaType.Integer | JsonSchemaType.Null
                : JsonSchemaType.Integer;
            schema.Format = underlyingType == typeof(long) ? "int64" : "int32";
        }

        return Task.CompletedTask;
    }

    private static bool IsIntegerType(Type type) =>
        type == typeof(int)
        || type == typeof(long)
        || type == typeof(short)
        || type == typeof(byte);
}
