using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace HalcyonRecords.Api.Common.OpenApi;

public static class OpenApiOperationExtensions
{
    public static void SetParameterExample(
        this OpenApiOperation operation,
        string parameterName,
        JsonNode example
    )
    {
        var parameter = (OpenApiParameter)
            operation.Parameters!.Single(p => p.Name == parameterName);
        parameter.Example = example;
    }

    public static void SetParameterRange(
        this OpenApiOperation operation,
        string parameterName,
        int minimum,
        int maximum
    )
    {
        var parameter = (OpenApiParameter)
            operation.Parameters!.Single(p => p.Name == parameterName);
        var schema = (OpenApiSchema)parameter.Schema!;
        schema.Minimum = minimum.ToString(CultureInfo.InvariantCulture);
        schema.Maximum = maximum.ToString(CultureInfo.InvariantCulture);
    }

    public static void SetParameterDefault(
        this OpenApiOperation operation,
        string parameterName,
        JsonNode defaultValue
    )
    {
        var parameter = (OpenApiParameter)
            operation.Parameters!.Single(p => p.Name == parameterName);
        var schema = (OpenApiSchema)parameter.Schema!;
        schema.Default = defaultValue;
    }

    public static void SetParameterArrayConstraints(
        this OpenApiOperation operation,
        string parameterName,
        int maxItems,
        int itemMaxLength
    )
    {
        var parameter = (OpenApiParameter)
            operation.Parameters!.Single(p => p.Name == parameterName);
        var schema = (OpenApiSchema)parameter.Schema!;
        schema.MaxItems = maxItems;
        ((OpenApiSchema)schema.Items!).MaxLength = itemMaxLength;
    }
}
