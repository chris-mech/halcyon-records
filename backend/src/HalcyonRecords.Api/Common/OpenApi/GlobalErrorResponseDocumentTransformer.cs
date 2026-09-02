using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi;

namespace HalcyonRecords.Api.Common.OpenApi;

public sealed class GlobalErrorResponseDocumentTransformer : IOpenApiDocumentTransformer
{
    public static readonly IReadOnlyList<int> DocumentedStatusCodes =
    [
        StatusCodes.Status400BadRequest,
        StatusCodes.Status408RequestTimeout,
        StatusCodes.Status415UnsupportedMediaType,
        StatusCodes.Status500InternalServerError,
        StatusCodes.Status503ServiceUnavailable,
    ];

    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var problemSchema = await context.GetOrCreateSchemaAsync(
            typeof(ProblemDetails),
            cancellationToken: cancellationToken
        );

        var operations = document.Paths.Values.SelectMany(pathItem =>
            pathItem.Operations?.Values ?? Enumerable.Empty<OpenApiOperation>()
        );

        foreach (var operation in operations)
        {
            operation.Responses ??= [];

            foreach (var statusCode in DocumentedStatusCodes)
            {
                if (
                    statusCode == StatusCodes.Status415UnsupportedMediaType
                    && operation.RequestBody is null
                )
                {
                    continue;
                }

                var key = statusCode.ToString(CultureInfo.InvariantCulture);

                if (operation.Responses.ContainsKey(key))
                {
                    continue;
                }

                operation.Responses[key] = new OpenApiResponse
                {
                    Description = ReasonPhrases.GetReasonPhrase(statusCode),
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/problem+json"] = new OpenApiMediaType
                        {
                            Schema = problemSchema,
                        },
                    },
                    Headers =
                        statusCode == StatusCodes.Status503ServiceUnavailable
                            ? new Dictionary<string, IOpenApiHeader>
                            {
                                ["Retry-After"] = new OpenApiHeader
                                {
                                    Description = "Seconds to wait before retrying.",
                                    Schema = new OpenApiSchema { Type = JsonSchemaType.Integer },
                                },
                            }
                            : null,
                };
            }
        }
    }
}
