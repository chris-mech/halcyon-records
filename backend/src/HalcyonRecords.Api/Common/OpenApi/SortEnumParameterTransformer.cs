using System.Text.Json.Nodes;
using HalcyonRecords.Api.Features.Albums.GetAlbums;
using HalcyonRecords.Api.Features.Artists.GetArtistById;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HalcyonRecords.Api.Common.OpenApi;

public sealed class SortEnumParameterTransformer : IOpenApiOperationTransformer
{
    private static readonly IReadOnlyDictionary<string, Type> s_sortEnumsByOperationId =
        new Dictionary<string, Type>
        {
            ["GetAlbums"] = typeof(AlbumSortBy),
            ["GetArtistById"] = typeof(ArtistAlbumSortBy),
        };

    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        if (
            operation.OperationId is not { } operationId
            || !s_sortEnumsByOperationId.TryGetValue(operationId, out var enumType)
        )
        {
            return Task.CompletedTask;
        }

        if (
            operation.Parameters?.FirstOrDefault(p => p.Name == "sort")
            is not OpenApiParameter sortParameter
        )
        {
            return Task.CompletedTask;
        }

        if (sortParameter.Schema is not OpenApiSchema schema)
        {
            return Task.CompletedTask;
        }

        schema.Type = JsonSchemaType.String;
        schema.Enum = Enum.GetNames(enumType)
            .Select(name => (JsonNode)JsonValue.Create(name))
            .ToList();

        return Task.CompletedTask;
    }
}
