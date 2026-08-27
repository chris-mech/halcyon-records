using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HalcyonRecords.Api.Common.OpenApi;

public sealed class WellKnownPropertyMinimumSchemaTransformer : IOpenApiSchemaTransformer
{
    private static readonly Dictionary<string, int> s_minimumsByPropertyName = new()
    {
        ["Quantity"] = 1,
    };

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (
            context.JsonPropertyInfo?.AttributeProvider is PropertyInfo propertyInfo
            && s_minimumsByPropertyName.TryGetValue(propertyInfo.Name, out var minimum)
        )
        {
            schema.Minimum = minimum.ToString(CultureInfo.InvariantCulture);
        }

        return Task.CompletedTask;
    }
}
