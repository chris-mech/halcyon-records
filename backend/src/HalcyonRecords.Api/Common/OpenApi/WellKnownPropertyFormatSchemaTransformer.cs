using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HalcyonRecords.Api.Common.OpenApi;

public sealed class WellKnownPropertyFormatSchemaTransformer : IOpenApiSchemaTransformer
{
    private static readonly Dictionary<string, string> s_formatsByPropertyName = new()
    {
        ["Email"] = "email",
        ["ContactEmail"] = "email",
        ["ImageUrl"] = "uri",
    };

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (
            context.JsonPropertyInfo?.AttributeProvider is PropertyInfo propertyInfo
            && s_formatsByPropertyName.TryGetValue(propertyInfo.Name, out var format)
        )
        {
            schema.Format = format;
        }

        return Task.CompletedTask;
    }
}
