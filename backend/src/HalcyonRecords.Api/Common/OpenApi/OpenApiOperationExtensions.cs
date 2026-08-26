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
}
